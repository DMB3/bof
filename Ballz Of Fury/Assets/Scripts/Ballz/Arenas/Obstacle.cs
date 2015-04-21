using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ballz.Arenas {
    
    /// <summary>
    /// Serializable info about an obstacle.
    /// </summary>
    public class Obstacle {

        public enum ObstacleShape {
            Cube,
            Sphere
        }

        public ObstacleShape Shape {
            get;
            set;
        }

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

        public String PhysicsMaterial {
            get;
            set;
        }

    }

}
