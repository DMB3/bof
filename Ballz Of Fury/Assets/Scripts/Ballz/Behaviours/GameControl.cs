using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ballz.Behaviours {

    /// <summary>
    /// For now this MonoBehaviour only contains a method to apply impulses to Ballz.
    /// </summary>
    public class GameControl : MonoBehaviour {

        private List<GameObject> allBalls;

        void Start() {
            this.allBalls = new List<GameObject>();

            foreach (Rigidbody body in GameObject.FindObjectsOfType<Rigidbody>() as Rigidbody[]) {
                if (body.tag.Equals("Ball")) {
                    this.allBalls.Add(body.gameObject);
                }
            }
        }

        public void ApplyImpulses() {
            foreach (GameObject ball in this.allBalls) {
                BallInput input = ball.GetComponent<BallInput>();
                Vector3 impulse = input.AppliedImpulse;
                ball.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
                input.ClearInputArrow();
            }
            this.GetComponent<AllSleeping>().OnAllSleeping += this.ArenaSleeping;
            this.GetComponent<AllSleeping>().StartCheck();
        }

        public void ArenaSleeping() {
            this.GetComponent<AllSleeping>().OnAllSleeping -= this.ArenaSleeping;
        }

    }

}
