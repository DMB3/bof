using UnityEngine;
using System.Collections;

namespace Ballz.Behaviours {

    public class BallInput : MonoBehaviour {

        public GameObject point;
        public float maxImpulse = 10f;
        internal Vector2 direction;
    
        private GameObject arrow = null;
        private Coroutine pointGen = null;
    
        public void OnMouseDown() {
            if (this.arrow == null) {
                this.arrow = new GameObject("arrow");
                this.arrow.transform.parent = this.transform;
                this.arrow.transform.localPosition = Vector3.zero;
            }
            if (this.pointGen != null) {
                this.StopCoroutine(this.pointGen);
            }
            this.pointGen = this.StartCoroutine(this.GeneratePoints());
        }

        private IEnumerator GeneratePoints(){
            while (true) {
                yield return new WaitForSeconds(0.25f);
                var point = GameObject.Instantiate(this.point) as GameObject;
                point.transform.parent = this.arrow.transform;
                var pointBehavior = point.GetComponent<PointBehaviour>();
                pointBehavior.parent = this;
            }
        }

        public void OnMouseDrag() {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = new RaycastHit();
            LayerMask mask = 1 << LayerMask.NameToLayer("Floor");
            if (!Physics.Raycast(ray, out hit, float.MaxValue, mask)) {
                return;
            }
            var dx = hit.point.x - this.transform.position.x;
            var dz = hit.point.z - this.transform.position.z;
            var angle = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;
            // Unity's y-axis zero (angle) is aligned with the z-axis; the x-axis corresponds to 90 degrees 
            // (therefore is positive clockwise), and Mathf.Atan2 is positive ccw, so we have to invert the angle 
            // returned by Atan2 and add 90 degrees to match 0 to the x-axis.
            this.arrow.transform.rotation = Quaternion.Euler(0, 90 - angle, 0);
            this.direction = new Vector2(dx, dz);
            if (this.direction.magnitude > this.maxImpulse) {
                this.direction *= this.maxImpulse / this.direction.magnitude;
            }
        }

        public void OnMouseUp() {
            // store impulse in ball
            Vector3 force = new Vector3(this.direction.x, 0.0f, this.direction.y);
            this.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        }
 
    }

}