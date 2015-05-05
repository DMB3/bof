using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Ballz.Behaviours {

    public class Timer : MonoBehaviour {

        public bool IsServer = true;
        public float Duration = 10;
        public Text Label;

        private GameControl gameControl;
        private Coroutine countDown;

        public delegate void OnExpiredCallback();
        public event OnExpiredCallback OnExpired;

        // Use this for initialization
        void Start() {
            if (this.IsServer) {
                var allSleeping = this.gameObject.GetComponent<AllSleeping>();
                allSleeping.OnAllSleeping += this.SendStateToClients;
            }
            this.gameControl = this.gameObject.GetComponent<GameControl>();
            this.StartCountDown();
        }

        public void Interrupt() {
            this.StopCoroutine(this.countDown);
            this.countDown = null;
            this.Label.text = "";
        }

        public void StartCountDown() {
            if (this.countDown != null) {
                this.Interrupt();
            }
            this.countDown = this.StartCoroutine(this.DoCountDown());
        }

        public IEnumerator DoCountDown() {
            float remaining = this.Duration;
            while (remaining > 0) {
                this.Label.text = string.Format("{0:0.0}", remaining);
                yield return null;
                remaining -= Time.deltaTime;
            }
            this.Label.text = "";
            if (this.IsServer && this.OnExpired != null) {
                // another component should register ProcessTurn() as a callback for this event
                this.OnExpired();  //this.ProcessTurn();
            }
        }


        // TODO: MOVE METHODS BELOW TO ANOTHER COMPONENT

        /// <summary>
        /// This method processes the turn on the server's side
        /// </summary>
        void ProcessTurn() {
            this.SendImpulsesToClients();
            this.gameControl.ApplyImpulses();
        }

        /// <summary>
        /// Send all impulses (stored in '???') to all clients so that they can display their own simulation of the turn
        /// </summary>
        void SendImpulsesToClients() {

        }

        /// <summary>
        /// Send the final state (object positions and rotations mainly) to all clients, so that they can fix the positions 
        /// and everyone sees the same game state before the next turn begins
        /// </summary>
        void SendStateToClients() {
               
        }
    }
}