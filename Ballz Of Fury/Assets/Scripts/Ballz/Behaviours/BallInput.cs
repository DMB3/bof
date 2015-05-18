using UnityEngine;
using System.Collections;

namespace Ballz.Behaviours {

    /// <summary>
    /// Class with MonoBehaviour handling mouse input over the ball.
    /// </summary>
    public class BallInput : MonoBehaviour {

        public GameObject Point;
        public float MaxImpulse = 10f;
        public int PlayerID;

        public Vector3 AppliedImpulse;

        internal Vector2 direction;
    
        private GameObject arrow = null;
        private Coroutine pointGen = null;

        internal string Name {
            get;
            set;
        }

        public void ClearInputArrow() {
            if (this.arrow != null) {
                GameObject.Destroy(this.arrow);
                this.arrow = null;
            }

            this.AppliedImpulse = Vector3.zero;
        }
    
        public void OnMouseDown() {
            if (!this.enabled) {
                // unfortunately, disabled objects still callback on OnMouse* :(
                return;
            }

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
            while (this.arrow != null) {
                var point = GameObject.Instantiate(this.Point) as GameObject;
                point.transform.parent = this.arrow.transform;
                var pointBehavior = point.GetComponent<PointBehaviour>();
                pointBehavior.parent = this;
                yield return new WaitForSeconds(0.25f);
            }
        }

        public void OnMouseDrag() {
            if (!this.enabled) {
                // unfortunately, disabled objects still callback on OnMouse* :(
                return;
            }

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
            if (this.direction.magnitude > this.MaxImpulse) {
                this.direction *= this.MaxImpulse / this.direction.magnitude;
            }
        }

        public void OnMouseUp() {
            if (!this.enabled) {
                // unfortunately, disabled objects still callback on OnMouse* :(
                return;
            }

            // store impulse in ball
            this.AppliedImpulse = new Vector3(this.direction.x, 0.0f, this.direction.y);
            if (Network.isClient) {
                this.SendImpulseToServer();
            }
        }

        /// <summary>
        /// Send this.AppliedImpulse to server along with the ball's ID.
        /// </summary>
        private void SendImpulseToServer() {
            GameObject.FindObjectOfType<NetworkManager>().GetComponent<NetworkView>().RPC("SetBallImpulse", RPCMode.All, this.Name, this.AppliedImpulse);
        }
 
    }

}