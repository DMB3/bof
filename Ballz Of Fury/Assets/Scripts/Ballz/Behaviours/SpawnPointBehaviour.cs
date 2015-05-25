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

        private GameObject spawnedBall;

        public void Spawn() {
            if (this.spawnedBall == null) {
                switch (this.Ball) {
                    case BallType.Attacker:
                        this.spawnedBall = GameObject.Instantiate(this.AttackerPrefab);
                        break;
                    case BallType.Defender:
                        this.spawnedBall = GameObject.Instantiate(this.DefenderPrefab);
                        break;
                    case BallType.Midfielder:
                        this.spawnedBall = GameObject.Instantiate(this.MidfielderPrefab);
                        break;
                    default:
                        throw new Exception("Invalid ball type " + this.Ball);
                }

                this.spawnedBall.GetComponent<BallInput>().PlayerID = this.PlayerID;
                this.spawnedBall.GetComponent<BallInput>().Name = String.Format("{0}-{1}", this.PlayerID, this.name);
                this.spawnedBall.GetComponent<BallInput>().spawnPoint = this;
                this.spawnedBall.GetComponent<MeshRenderer>().material.color = this.PlayerColours.Evaluate(this.PlayerID / 10.0f);
            }

            this.spawnedBall.transform.position = this.transform.position;
            this.spawnedBall.transform.rotation = this.transform.rotation;
            this.spawnedBall.transform.parent = this.transform.parent;
        }

    }

}
