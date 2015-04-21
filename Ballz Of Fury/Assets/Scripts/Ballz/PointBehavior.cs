using UnityEngine;
using System.Collections;


namespace Ballz {
    public class PointBehavior : MonoBehaviour {
        internal BallInput parent;

        void Start() {
            this.transform.localPosition = Vector3.zero;
        }

        void Update() {
            var distance = this.parent.direction.magnitude;
            var target = new Vector3(0, 0, distance);
            this.transform.localPosition += Vector3.forward * distance * Time.deltaTime;
            if (this.transform.localPosition.z >= distance) {
                this.transform.localPosition = target;
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}