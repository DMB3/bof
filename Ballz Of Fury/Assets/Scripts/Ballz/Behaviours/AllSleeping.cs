using UnityEngine;
using System.Collections;


namespace Ballz.Behaviours {

    public class AllSleeping : MonoBehaviour {

        public delegate void AllSleepingCallback();
        public event AllSleepingCallback OnAllSleeping;

        void Start() {
            this.OnAllSleeping += this.IsAllzSleepingz;
            this.StartCheck();
        }

        void StartCheck() {
            this.StartCoroutine(this.Check());
        }

        IEnumerator Check() {
            Rigidbody[] rigidBodies = GameObject.FindObjectsOfType<Rigidbody>() as Rigidbody[];
            bool allSleeping = false;
            while(!allSleeping) {
                allSleeping = true;
                foreach (Rigidbody rigidBody in rigidBodies) {
                    if(!rigidBody.IsSleeping()) {
                        allSleeping = false;
                        yield return null;
                        break;
                    }
                }
            }
            this.OnAllSleeping();
        }

        private void IsAllzSleepingz() {
            //AllSleeping.print("PUTAAAAAA!");
        }

    }

}