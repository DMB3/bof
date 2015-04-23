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

        public Button ApplyImpulseButton;

        public void ApplyImpulses() {
            this.ApplyImpulseButton.gameObject.SetActive(false);
            foreach (Rigidbody body in GameObject.FindObjectsOfType<Rigidbody>() as Rigidbody[]) {
                if (body.tag.Equals("Ball")) {
                    BallInput input = body.GetComponent<BallInput>();
                    Vector3 impulse = input.AppliedImpulse;
                    body.AddForce(impulse, ForceMode.Impulse);
                    input.ClearInputArrow();
                    input.enabled = false;
                }
            }
            this.GetComponent<AllSleeping>().OnAllSleeping += this.ArenaSleeping;
            this.GetComponent<AllSleeping>().StartCheck();
        }

        public void ArenaSleeping() {
            this.ApplyImpulseButton.gameObject.SetActive(true);
            this.GetComponent<AllSleeping>().OnAllSleeping -= this.ArenaSleeping;
            foreach (Rigidbody body in GameObject.FindObjectsOfType<Rigidbody>() as Rigidbody[]) {
                if (body.tag.Equals("Ball")) {
                    BallInput input = body.GetComponent<BallInput>();
                    input.enabled = true;
                }
            }
        }

    }

}
