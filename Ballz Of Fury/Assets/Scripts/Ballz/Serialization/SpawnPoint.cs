using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ballz.Serialization {

    /// <summary>
    /// Serializable info about a spawn point for a ball.
    /// </summary>
    public class SpawnPoint {

        public Vector3 Position {
            get;
            set;
        }

        public Quaternion Rotation {
            get;
            set;
        }

        public int PlayerID {
            get;
            set;
        }

        public Ballz.Behaviours.SpawnPointBehaviour.BallType Ball {
            get;
            set;
        }

    }

}
