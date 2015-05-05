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

        public Text RemoteIPText;
        public Text LocalIPText;

        public Transform MainMenuPanel;
        public Transform StandByPanel;
        public Text StandByText;
        public Button CloseStandByButton;
        public GameObject ArenaParent;

        void Start() {
            // fill in the textboxes with our public IP address, if we have one, or the LAN address otherwise
            this.LocalIPText.text = (Network.HavePublicAddress() ? Network.player.externalIP : Network.player.ipAddress);
            this.RemoteIPText.text = this.LocalIPText.text;
            // display the main menu panel
            this.ShowMainMenu();
        }

        public void ConnectToServer() {
            // attempt connection
            NetworkConnectionError error = Network.Connect(this.RemoteIPText.text, NetworkManager.SERVER_PORT);
            // show stand by message while waiting
            this.ShowStandBy();
            this.StandByText.text = "Attempting to connect to remote server, please wait...";

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

        public void StartServer() {
            // attempt to start server
            NetworkConnectionError error = Network.InitializeServer(NetworkManager.MAXIMUM_CONNECTIONS, NetworkManager.SERVER_PORT, !Network.HavePublicAddress());
            // show stand by message while waiting
            this.ShowStandBy();
            this.StandByText.text = "Attempting to start server, please wait...";

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

        public void CloseStandBy() {
            Network.Disconnect();
            this.ShowMainMenu();
        }

        void OnConnectedToServer() {
            this.StandByText.text = "Connected to server.\n\nStarting game!";
            this.StartGame();
        }

        void OnFailedToConnect(NetworkConnectionError error) {
            this.StandByText.text = string.Format("Failed to connect to server.\n{0}", error.ToString());
            this.CloseStandByButton.gameObject.SetActive(true);
        }

        void OnServerInitialized() {
            this.StandByText.text = string.Format("Server initialized! Waiting for opponent...\nYour IP address is {0}", this.LocalIPText.text);
            this.CloseStandByButton.gameObject.SetActive(true);
        }

        void OnPlayerConnected(NetworkPlayer player) {
            this.StandByText.text = string.Format("Player connected!\nPlayer IP address is {0}:{1}.\n\nStarting game!", player.ipAddress, player.port);
            this.StartGame();
        }

        private void ShowStandBy() {
            this.ArenaParent.SetActive(false);
            this.MainMenuPanel.gameObject.SetActive(false);
            this.StandByPanel.gameObject.SetActive(true);
            this.CloseStandByButton.gameObject.SetActive(false);
        }

        private void ShowMainMenu() {
            this.ArenaParent.SetActive(false);
            this.MainMenuPanel.gameObject.SetActive(true);
            this.StandByPanel.gameObject.SetActive(false);
        }

        private void ShowArena() {
            this.ArenaParent.SetActive(true);
            this.MainMenuPanel.gameObject.SetActive(false);
            this.StandByPanel.gameObject.SetActive(false);
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

            // TODO: actually start the game (the Timer component of this.ArenaParent should be activated as well)
        }

    }

}