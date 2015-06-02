using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ballz.Serialization {

    /// <summary>
    /// Serializable class that defines the layout of an arena.
    /// This can be used to store and retrieve arenas from files that will then be created programatically.
    /// </summary>
    public class Arena {

        public String Name {
            get;
            set;
        }

        public int MaximumPlayers {
            get;
            set;
        }

        public List<Obstacle> Obstacles {
            get;
            set;
        }

        public List<SpawnPoint> SpawnPoints {
            get;
            set;
        }

        public List<Goal> Goals {
            get;
            set;
        }

        public List<Ballz.Serialization.Light> Lights {
            get;
            set;
        }

        public Floor Floor {
            get;
            set;
        }

    }

}
