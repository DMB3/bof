using UnityEngine;
using System.Collections;
using System;


namespace Ballz.Behaviours {

    /// <summary>
    /// Class with MonoBehaviour handling mouse input over the ball.
    /// </summary>
    public class BallInput : MonoBehaviour {

        public GameObject Point;
        public float MaxSpeed = 50;
        public float MaxImpulseRadius = 1;
        public int PlayerID;
        public int ScoreValue = 1;

        public Vector3 AppliedImpulse;

        internal Vector2 direction;
        internal SpawnPointBehaviour spawnPoint;
    
        private GameObject arrow = null;
        private GameObject circle = null;
        private Coroutine pointGen = null;
        internal float MaxImpulse;  // PointBehavior uses this field

        internal string Name {
            get;
            set;
        }

        void Start() {
            Rigidbody rigidBody = this.GetComponent<Rigidbody>();
            this.MaxImpulse = rigidBody.mass * this.MaxSpeed;
        }

        public void ClearInputArrow() {
            if (this.arrow != null) {
                GameObject.Destroy(this.arrow);
                this.arrow = null;
                this.circle = null;
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
                this.circle = new GameObject("circle");
                this.circle.transform.parent = this.arrow.transform;
                this.circle.transform.localPosition = Vector3.zero;
                this.CreateCircle();
            }
            if (this.pointGen != null) {
                this.StopCoroutine(this.pointGen);
            }
            this.pointGen = this.StartCoroutine(this.GeneratePoints());
        }

        private void CreateCircle() {
            var numPoints = 20f;
            for (int i = 0; i < numPoints; i++) {
                var angle = (i / numPoints) * 2 * Math.PI;
                var xPos = this.MaxImpulseRadius * Math.Cos(angle);
                var zPos = this.MaxImpulseRadius * Math.Sin(angle);
                var point = GameObject.Instantiate(this.Point) as GameObject;
                point.transform.parent = this.circle.transform;
                point.transform.localPosition = new Vector3((float) xPos, 0, (float) zPos);
                // we must disable the point behavior in this case because these points will be stationary
                var pointBehavior = point.GetComponent<PointBehaviour>();
                pointBehavior.enabled = false;
            }   
        }

        private void ClearCircle() {
            if (this.circle != null) {
                GameObject.Destroy(this.circle);
                this.circle = null;
            }
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
            if (!this.enabled || this.arrow == null) {
                // unfortunately, disabled objects still callback on OnMouse* :(
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = new RaycastHit();
            LayerMask mask = 1 << LayerMask.NameToLayer("Floor");
            if (!Physics.Raycast(ray, out hit, float.MaxValue, mask)) {
                return;
            }
            float ballY = this.transform.position.y;
            float lambda = (ballY - hit.point.y) / ray.direction.y;
            float ux = hit.point.x + lambda * ray.direction.x;
            float uz = hit.point.z + lambda * ray.direction.z;

            var dx = ux - this.transform.position.x;
            var dz = uz - this.transform.position.z;
            var angle = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;
            // Unity's y-axis zero (angle) is aligned with the z-axis; the x-axis corresponds to 90 degrees 
            // (therefore is positive clockwise), and Mathf.Atan2 is positive ccw, so we have to invert the angle 
            // returned by Atan2 and add 90 degrees to match 0 to the x-axis.
            this.arrow.transform.rotation = Quaternion.Euler(0, 90 - angle, 0);
            this.direction = new Vector2(dx, dz);
            if (this.direction.magnitude > this.MaxImpulseRadius) {
                this.direction *= this.MaxImpulseRadius / this.direction.magnitude;
            }
        }

        public void OnMouseUp() {
            if (!this.enabled || this.arrow == null) {
                // unfortunately, disabled objects still callback on OnMouse* :(
                return;
            }
            // destroy the circle showing the maximum impulse (but keep the arrow)
            //this.ClearCircle();

            // store impulse in ball
            this.AppliedImpulse = new Vector3(this.direction.x, 0.0f, this.direction.y);
            // the following operation converts the direction 
            this.AppliedImpulse *= this.MaxImpulse / this.MaxImpulseRadius;
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

        /// <summary>
        /// Mark this ball as having scored a goal.
        /// </summary>
        internal void MarkGoal() {
            if (Network.isServer) {
                GameObject.FindObjectOfType<NetworkManager>().GetComponent<NetworkView>().RPC("AddGoal", RPCMode.All, this.PlayerID, this.ScoreValue);
            }
        }
 
    }

}