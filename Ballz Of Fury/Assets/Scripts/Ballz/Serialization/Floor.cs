using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ballz.Serialization {

    /// <summary>
    /// Serializable info about the floor.
    /// </summary>
    public class Floor {

        public Vector3 Position {
            get;
            set;
        }

        public Vector3 Scale {
            get;
            set;
        }

        public String RendererMaterialName {
            get;
            set;
        }

    }

}
