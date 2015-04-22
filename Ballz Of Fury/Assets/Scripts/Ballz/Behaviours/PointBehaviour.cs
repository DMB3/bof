using UnityEngine;
using System.Collections;

namespace Ballz.Behaviours {

    /// <summary>
    /// Class with MonoBehaviour handling each of the points that composes the ball impulse "arrow".
    /// These points "handle themselves" by automatically moving to the arrow's target position.
    /// </summary>
    public class PointBehaviour : MonoBehaviour {

        public float TimeToTarget = 1;
        public Gradient Gradient;

        private float startTime;
        internal BallInput parent;

        void Start() {
            this.transform.localPosition = Vector3.zero;
            this.startTime = Time.time;
        }

        void Update() {
            var progress = Mathf.Clamp01((Time.time - this.startTime) / this.TimeToTarget);
            var distance = this.parent.direction.magnitude;
            this.transform.localPosition = new Vector3(0, 0, progress * distance);

            // set color according to impulse given
            var power = this.parent.direction.magnitude / this.parent.MaxImpulse;
            this.GetComponent<Renderer>().material.color = this.Gradient.Evaluate(power);

            if (progress == 1) {
                GameObject.Destroy(this.gameObject);
            }
        }

    }

}