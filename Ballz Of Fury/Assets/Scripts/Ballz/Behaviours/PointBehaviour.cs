using UnityEngine;
using System.Collections;


namespace Ballz.Behaviours {

    public class PointBehaviour : MonoBehaviour {

        public float timeToTarget = 1;
        public Gradient gradient;
        private float startTime;
        internal BallInput parent;

        void Start() {
            this.transform.localPosition = Vector3.zero;
            this.startTime = Time.time;
        }

        void Update() {
            var progress = Mathf.Clamp01((Time.time - this.startTime) / this.timeToTarget);
            var distance = this.parent.direction.magnitude;
            this.transform.localPosition = new Vector3(0, 0, progress * distance);

            // set color according to impulse given
            var power = this.parent.direction.magnitude / this.parent.maxImpulse;
            this.GetComponent<Renderer>().material.color = this.gradient.Evaluate(power);

            if (progress == 1) {
                GameObject.Destroy(this.gameObject);
            }
        }

    }

}