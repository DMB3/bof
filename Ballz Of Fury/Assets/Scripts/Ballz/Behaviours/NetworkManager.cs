using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Ballz.Behaviours {

    /// <summary>
    /// Behaviour for controlling networking basics.
    /// </summary>
    public class NetworkManager : MonoBehaviour {

        private const int MAXIMUM_CONNECTIONS = 2;
        private const int SERVER_PORT = 9999;

        private bool initialized;

        public Text RemoteIPText;
        public Text LocalIPText;
        public Text StandByText;
        public Button CloseStandByButton;
        public Button ReadyImpulsesButton;

        public Transform MainMenuPanel;
        public Transform StandByPanel;
        public Transform GameUI;

        public GameObject ArenaParent;

        void Start() {
            this.initialized = false;

            // fill in the textboxes with our public IP address, if we have one, or the LAN address otherwise
            this.LocalIPText.text = (Network.HavePublicAddress() ? Network.player.externalIP : Network.player.ipAddress);
            this.RemoteIPText.text = this.LocalIPText.text;

            // display the main menu panel
            this.ShowMainMenu();
        }

        public void ConnectToServer() {
            // show stand by message while waiting
            this.ShowStandBy();
            this.StandByText.text = "Attempting to connect to remote server, please wait...";

            // attempt connection
            NetworkConnectionError error = Network.Connect(this.RemoteIPText.text, NetworkManager.SERVER_PORT);

            if (!this.initialized) {
                if (error == NetworkConnectionError.NoError) {
                    // connection attempt is successful
                    // note that this does not mean that we have actually connected! if we have, then 
                    // OnConnectedToServer will be called, otherwise OnFailedtoConnect will be called
                    this.StandByText.text = "Connecting to remote server, please wait...";
                } else {
                    // something happened, deal with this further down the line
                    // for now we're going to print the error to log
                    this.StandByText.text = error.ToString();
                    this.CloseStandByButton.gameObject.SetActive(true);
                }
            }
        }

        public void StartServer() {
            // show stand by message while waiting
            this.ShowStandBy();
            this.StandByText.text = "Attempting to start server, please wait...";

            // attempt to start server
            NetworkConnectionError error = Network.InitializeServer(NetworkManager.MAXIMUM_CONNECTIONS, NetworkManager.SERVER_PORT, !Network.HavePublicAddress());

            if (!this.initialized) {
                if (error == NetworkConnectionError.NoError) {
                    // start attempt is sucessful
                    // note that this does not mean that we have actually started the server! if we have, then
                    // OnServerInitialized will be called
                    this.StandByText.text = "Starting server, please wait...";
                } else {
                    // something happened, deal with this further down the line
                    // for now we're going to print the error to log
                    this.StandByText.text = error.ToString();
                    this.CloseStandByButton.gameObject.SetActive(true);
                }
            }
        }

        public void CloseStandBy() {
            Network.Disconnect();
            this.ShowMainMenu();
        }

        void OnConnectedToServer() {
            // note that if things happen fast enough this can occur *BEFORE* the rest of the code in ConnectToServer
            // executes after Network.Connect; the 'initialized' variable is used to control if this has actually
            // already executed
            this.initialized = true;
            this.StandByText.text = "Connected to server.\n\nStarting game!";
            this.StartGame();
        }

        void OnFailedToConnect(NetworkConnectionError error) {
            this.StandByText.text = string.Format("Failed to connect to server.\n{0}", error.ToString());
            this.CloseStandByButton.gameObject.SetActive(true);
        }

        void OnServerInitialized() {
            // note that if things happen fast enough this can occur *BEFORE* the rest of the code in StartServer
            // executes after Network.InitializeServer; the 'initialized' variable is used to control if this has actually
            // already executed
            this.initialized = true;
            this.StandByText.text = string.Format("Server initialized! Waiting for opponent...\nYour IP address is {0}", this.LocalIPText.text);
            this.CloseStandByButton.gameObject.SetActive(true);
        }

        void OnPlayerConnected(NetworkPlayer player) {
            this.StandByText.text = string.Format("Player connected!\nPlayer IP address is {0}:{1}.\n\nStarting game!", player.ipAddress, player.port);
            this.StartGame();
        }

        void OnPlayerDisconnected(NetworkPlayer player) {
            this.ShowStandBy();
            this.StandByText.text = string.Format("Player at {0} disconnected! :(", player.ipAddress);
            Network.RemoveRPCs(player);
            this.CloseStandByButton.gameObject.SetActive(true);
        }

        private void ShowStandBy() {
            this.ArenaParent.SetActive(false);
            this.MainMenuPanel.gameObject.SetActive(false);
            this.StandByPanel.gameObject.SetActive(true);
            this.CloseStandByButton.gameObject.SetActive(false);
            this.GameUI.gameObject.SetActive(false);
        }

        private void ShowMainMenu() {
            this.ArenaParent.SetActive(false);
            this.MainMenuPanel.gameObject.SetActive(true);
            this.StandByPanel.gameObject.SetActive(false);
            this.GameUI.gameObject.SetActive(false);
        }

        private void ShowArena() {
            this.ArenaParent.SetActive(true);
            this.MainMenuPanel.gameObject.SetActive(false);
            this.StandByPanel.gameObject.SetActive(false);
            this.GameUI.gameObject.SetActive(false);
        }

        private void StartGame() {
            // show the arena parent and load a new arena
            this.ShowArena();
            this.ArenaParent.GetComponent<ArenaSerializer>().LoadFromTargetFilePath();

            // prevent input on other player's objects
            foreach (Rigidbody body in GameObject.FindObjectsOfType<Rigidbody>() as Rigidbody[]) {
                if (body.tag.Equals("Ball")) {
                    BallInput input = body.GetComponent<BallInput>();
                    int myPlayer = (Network.isServer ? 0 : 1);
                    input.enabled = (input.PlayerID == myPlayer);
                }
            }

            this.GameUI.gameObject.SetActive(true);
            this.ArenaParent.GetComponent<Timer>().enabled = true;

            if (Network.isServer) {
                // if we are the server, we will register to the Timer's OnNotify and OnExpired event
                // we will use OnNotify to sync player's clocks (this is because doing stuff like dragging the window
                // or something may cause player timer's to stop) and we will use OnExpired to force impulses to be applied
                this.ArenaParent.GetComponent<Timer>().OnNotify += this.SynchronizeClocks;
                this.ArenaParent.GetComponent<Timer>().OnExpired += this.ProcessTurn;
                this.ArenaParent.GetComponent<AllSleeping>().OnAllSleeping += this.SendStateToClients;
            }
        }

        /// <summary>
        /// Sends current clock time left (Timer) to all players so that they synchronize their clock values.
        /// </summary>
        private void SynchronizeClocks() {
            this.GetComponent<NetworkView>().RPC("SetClockTimeLeft", RPCMode.All, this.ArenaParent.GetComponent<Timer>().RemainingDuration);
        }

        [RPC]
        private void SetClockTimeLeft(float timeLeft) {
            this.ArenaParent.GetComponent<Timer>().RemainingDuration = timeLeft;
        }

        /// <summary>
        /// This method processes the turn on the server's side.
        /// </summary>
        private void ProcessTurn() {
            this.SendImpulsesToClients();
            this.ArenaParent.GetComponent<GameControl>().ApplyImpulses();
        }

        /// <summary>
        /// Send all impulses to all clients so that they can display their own simulation of the turn.
        /// </summary>
        private void SendImpulsesToClients() {

        }

        /// <summary>
        /// Send the final state (object positions and rotations mainly) to all clients, so that they can fix the positions 
        /// and everyone sees the same game state before the next turn begins.
        /// </summary>
        private void SendStateToClients() {

        }

    }

}