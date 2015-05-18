using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ballz.Behaviours {

    /// <summary>
    /// This MonoBehaviour contains the functionality of Spawn Points.
    /// </summary>
    public class SpawnPointBehaviour : MonoBehaviour {

        public enum BallType {
            Defender,
            Midfielder,
            Attacker
        }

        public BallType Ball;
        public int PlayerID;

        public GameObject DefenderPrefab;
        public GameObject MidfielderPrefab;
        public GameObject AttackerPrefab;

        public Gradient PlayerColours;

        public void Spawn() {
            GameObject obj;

            switch (this.Ball) {
                case BallType.Attacker:
                    obj = GameObject.Instantiate(this.AttackerPrefab);
                    break;
                case BallType.Defender:
                    obj = GameObject.Instantiate(this.DefenderPrefab);
                    break;
                case BallType.Midfielder:
                    obj = GameObject.Instantiate(this.MidfielderPrefab);
                    break;
                default:
                    throw new Exception("Invalid ball type " + this.Ball);
            }

            obj.transform.position = this.transform.position;
            obj.transform.rotation = this.transform.rotation;
            obj.transform.parent = this.transform.parent;

            obj.GetComponent<BallInput>().PlayerID = this.PlayerID;
            obj.GetComponent<BallInput>().Name = String.Format("{0}-{1}", this.PlayerID, this.name);
            obj.GetComponent<MeshRenderer>().material.color = this.PlayerColours.Evaluate(this.PlayerID / 10.0f);
        }

    }

}
