using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ballz.Behaviours {

    /// <summary>
    /// This MonoBehaviour contains the functionality of Goals.
    /// </summary>
    public class GoalBehaviour : MonoBehaviour {

        public enum BallType {
            Defender,
            Midfielder,
            Attacker
        }

        public int PlayerID;
        public Gradient PlayerColours;

        void Update() {
            this.GetComponentInChildren<Light>().color = this.PlayerColours.Evaluate(this.PlayerID / 10.0f);
        }

    }

}
