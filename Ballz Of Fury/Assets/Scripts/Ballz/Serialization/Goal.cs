using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ballz.Serialization {

    /// <summary>
    /// Serializable info about a goal.
    /// </summary>
    public class Goal {

        public Vector3 Position {
            get;
            set;
        }

        public Quaternion Rotation {
            get;
            set;
        }

        public Vector3 Scale {
            get;
            set;
        }

        public int PlayerID {
            get;
            set;
        }

    }

}
