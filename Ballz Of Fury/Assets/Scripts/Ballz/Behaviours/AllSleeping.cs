using UnityEngine;
using System.Collections;


namespace Ballz.Behaviours {

    /// <summary>
    /// Class with MonoBehaviour to check if all balls in the arena are currently at rest.
    /// </summary>
    public class AllSleeping : MonoBehaviour {

        public delegate void AllSleepingCallback();
        public event AllSleepingCallback OnAllSleeping;

        public void StartCheck() {
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

    }

}