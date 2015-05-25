using UnityEngine;
using System.Collections;

namespace Ballz.Behaviours {

    public class GoalTriggerBehaviour : MonoBehaviour {

        private GoalBehaviour goal;

        void Start() {
            this.goal = this.GetComponentInParent<GoalBehaviour>();
        }

        void OnTriggerEnter(Collider other) {
            if (other.tag.Equals("Ball")) {
                BallInput input = other.GetComponent<BallInput>();
                if (!input.PlayerID.Equals(this.goal.PlayerID)) {
                    input.MarkGoal();
                }
            }
        }

    }

}