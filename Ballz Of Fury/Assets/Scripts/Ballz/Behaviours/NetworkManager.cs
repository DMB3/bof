using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Ballz.Behaviours {

    /// <summary>
    /// Behaviour for controlling networking basics.
    /// </summary>
    public class NetworkManager : MonoBehaviour {

        private const int MAXIMUM_CONNECTIONS = 2;
        private const int SERVER_PORT = 9999;

        private bool initialized;
        private bool simulating;
        private bool doRespawn;
        private bool gameEnded;

        private Dictionary<string, object[]> ballStates;

        private List<GameObject> allBalls;
        private List<GameObject> myBalls;
        private List<GameObject> opponentsBalls;

        public InputField RemoteIPText;
        public Text LocalIPText;
        public Text StandByText;
        public InputField ScoreLimitText;
        public Button CloseStandByButton;

        public Transform MainMenuPanel;
        public Transform StandByPanel;
        public Transform GameUI;

        public GameObject ArenaParent;

        public Transform YouScorePanel;
        public Transform OpponentScorePanel;
        public Text YouScoreText;
        public Text OpponentScoreText;
        public Text ScoreLimitIndicator;

        private GameControl control;
        private int scoreLimit;

        void Start() {
            this.initialized = false;
            this.control = this.ArenaParent.GetComponent<GameControl>();

            // display the main menu panel
            this.ShowMainMenu();

            // fill in the textboxes with our public IP address, if we have one, or the LAN address otherwise
            this.LocalIPText.text = (Network.HavePublicAddress() ? Network.player.externalIP : Network.player.ipAddress);
            this.RemoteIPText.text = this.LocalIPText.text;
            
            // fill in game setup stuff
            this.ScoreLimitText.text = "5";
        }

        void Update() {
            if (this.simulating) {
                this.YouScoreText.text = this.control.PlayerScore.ToString();
                this.OpponentScoreText.text = this.control.OpponentScore.ToString();
            }
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

            this.gameEnded = false;
            this.doRespawn = false;

            this.GameUI.gameObject.SetActive(true);

            this.FindBalls();

            this.ArenaParent.GetComponent<Timer>().enabled = true;
            this.BeginNewTurn();

            this.control.PlayerScore = 0;
            this.control.OpponentScore = 0;
            this.YouScorePanel.GetComponent<Image>().color = this.myBalls[0].GetComponent<MeshRenderer>().material.color;
            this.OpponentScorePanel.GetComponent<Image>().color = this.opponentsBalls[0].GetComponent<MeshRenderer>().material.color;

            if (Network.isServer) {
                // if we are the server, we will register to the Timer's OnNotify and OnExpired event
                // we will use OnNotify to sync player's clocks (this is because doing stuff like dragging the window
                // or something may cause player timer's to stop) and we will use OnExpired to force impulses to be applied
                this.ArenaParent.GetComponent<Timer>().OnNotify += this.SynchronizeClocks;
                this.ArenaParent.GetComponent<Timer>().OnExpired += this.ProcessTurn;
                this.ArenaParent.GetComponent<AllSleeping>().OnAllSleeping += this.SendStateToClients;
                this.GetComponent<NetworkView>().RPC("SetScoreLimit", RPCMode.All, int.Parse(this.ScoreLimitText.text));
            } else {
                this.ArenaParent.GetComponent<AllSleeping>().OnAllSleeping += this.SynchObjectStates;
            }
        }
        
        private void FindBalls() {
            this.allBalls = new List<GameObject>();
            this.myBalls = new List<GameObject>();
            this.opponentsBalls = new List<GameObject>();

            int myPlayer = (Network.isServer ? 0 : 1);
            foreach (Rigidbody body in GameObject.FindObjectsOfType<Rigidbody>() as Rigidbody[]) {
                if (body.tag.Equals("Ball")) {
                    this.allBalls.Add(body.gameObject);
                    BallInput input = body.GetComponent<BallInput>();
                    if (input.PlayerID == myPlayer) {
                        this.myBalls.Add(body.gameObject);
                    } else {
                        this.opponentsBalls.Add(body.gameObject);
                    }
                }
            }
        }

        [RPC]
        private void SetScoreLimit(int limit) {
            this.ScoreLimitIndicator.text = string.Format("Score Limit: {0}", limit);
            this.scoreLimit = limit;
        }

        [RPC]
        private void BeginNewTurn() {
            if (!this.gameEnded) {
                if (this.doRespawn) {
                    // respawn all balls
                    this.RespawnBalls();
                    this.doRespawn = false;
                }

                // restart the turn timer
                this.ArenaParent.GetComponent<Timer>().Reset();
                this.ArenaParent.GetComponent<Timer>().StartCountDown();

                // prevent input on other player's objects and allow input on our objects
                foreach (GameObject ball in this.myBalls) {
                    BallInput input = ball.GetComponent<BallInput>();
                    input.enabled = true;
                }
                foreach (GameObject ball in this.opponentsBalls) {
                    BallInput input = ball.GetComponent<BallInput>();
                    input.enabled = false;
                }

                // clean ball state cache
                this.ballStates = new Dictionary<string, object[]>();
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

        [RPC]
        private void SetBallImpulse(string ballName, Vector3 impulse) {
            foreach (GameObject ball in this.allBalls) {
                BallInput input = ball.GetComponent<BallInput>();
                if (input.Name.Equals(ballName)) {
                    input.AppliedImpulse = impulse;
                    break;
                }
            }
        }

        [RPC]
        private void ApplyAllImpulses() {
            // prevent impulses on balls
            foreach (GameObject ball in this.allBalls) {
                BallInput input = ball.GetComponent<BallInput>();
                input.enabled = false;
            }

            this.simulating = true;
            this.control.ApplyImpulses();
        }

        /// <summary>
        /// This method processes the turn on the server's side.
        /// </summary>
        private void ProcessTurn() {
            this.SendImpulsesToClients();
        }

        /// <summary>
        /// Send all impulses to all clients so that they can display their own simulation of the turn.
        /// </summary>
        private void SendImpulsesToClients() {
            foreach (GameObject ball in this.allBalls) {
                BallInput input = ball.GetComponent<BallInput>();
                this.GetComponent<NetworkView>().RPC("SetBallImpulse", RPCMode.All, input.Name, input.AppliedImpulse);
            }
            this.GetComponent<NetworkView>().RPC("ApplyAllImpulses", RPCMode.All);
        }

        private void SynchObjectStates() {
            this.simulating = false;

            foreach (KeyValuePair<string,object[]> keyValue in this.ballStates) {
                this.ApplyBallState(keyValue.Key, (Vector3)keyValue.Value[0], (Quaternion)keyValue.Value[1]);
            }

            this.ballStates = new Dictionary<string, object[]>();
        }

        private void ApplyBallState(string ballName, Vector3 position, Quaternion rotation) {
            foreach (GameObject ball in this.allBalls) {
                BallInput input = ball.GetComponent<BallInput>();
                if (input.Name.Equals(ballName)) {
                    ball.transform.position = position;
                    ball.transform.rotation = rotation;
                    break;
                }
            }
        }

        [RPC]
        private void ReceiveBallState(string ballName, Vector3 position, Quaternion rotation) {
            foreach (GameObject ball in this.allBalls) {
                BallInput input = ball.GetComponent<BallInput>();
                if (input.Name.Equals(ballName)) {
                    if (this.simulating) {
                        this.ballStates.Add(ballName, new object[] { position, rotation });
                    } else {
                        ball.transform.position = position;
                        ball.transform.rotation = rotation;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Send the final state (object positions and rotations mainly) to all clients, so that they can fix the positions 
        /// and everyone sees the same game state before the next turn begins.
        /// </summary>
        private void SendStateToClients() {
            foreach (GameObject ball in this.allBalls) {
                BallInput input = ball.GetComponent<BallInput>();
                this.GetComponent<NetworkView>().RPC("ReceiveBallState", RPCMode.All, input.Name, input.transform.position, input.transform.rotation);
            }

            this.GetComponent<NetworkView>().RPC("BeginNewTurn", RPCMode.All);
        }

        [RPC]
        private void MarkGameEnded() {
            this.gameEnded = true;
        }

        [RPC]
        private void AddGoal(int playerID, int scoreValue) {
            int myPlayer = (Network.isServer ? 0 : 1);
            if (playerID == myPlayer) {
                this.control.PlayerScore += scoreValue;
            } else {
                this.control.OpponentScore += scoreValue;
            }
            this.doRespawn = true;

            if (Network.isServer) {
                this.CheckScoreLimit();
            }
        }

        private void CheckScoreLimit() {
            if (this.control.PlayerScore >= this.scoreLimit || this.control.OpponentScore >= this.scoreLimit) {
                // game has ended, whenever next turn is set to begin - don't begin it :)
                this.GetComponent<NetworkView>().RPC("MarkGameEnded", RPCMode.All);
            }
        }

        private void RespawnBalls() {
            foreach (GameObject ball in this.allBalls) {
                ball.GetComponent<BallInput>().spawnPoint.Spawn();
            }
        }

    }

}