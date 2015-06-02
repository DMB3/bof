using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ballz.Serialization {

    /// <summary>
    /// Serializable class that defines a light.
    /// </summary>
    public class Light {

        public string Name {
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

        public LightType LightType {
            get;
            set;
        }

        public float Range {
            get;
            set;
        }

        public float SpotAngle {
            get;
            set;
        }

        public float Intensity {
            get;
            set;
        }

        public float BounceIntensity {
            get;
            set;
        }

        public Color Colour {
            get;
            set;
        }

    }

}
