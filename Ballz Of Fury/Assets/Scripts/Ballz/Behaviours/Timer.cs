using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Ballz.Behaviours {

    public class Timer : MonoBehaviour {

        public float Duration = 10;
        public float NotifyDuration = 1;
        public float RemainingDuration = 0;

        public Text Label;

        private Coroutine countDown;

        public delegate void OnExpiredCallback();
        public delegate void OnNotifyCallback();
        
        public event OnExpiredCallback OnExpired;
        public event OnNotifyCallback OnNotify;

        void Start() {
            this.StartCountDown();
        }

        public void Interrupt() {
            this.StopCoroutine(this.countDown);
            this.countDown = null;
            this.Label.text = "";
        }

        public void StartCountDown() {
            if (this.countDown != null) {
                this.Interrupt();
            }
            this.countDown = this.StartCoroutine(this.DoCountDown());
        }

        public IEnumerator DoCountDown() {
            this.RemainingDuration = this.Duration;
            float notify = this.NotifyDuration;

            while (this.RemainingDuration > 0) {
                this.Label.text = string.Format("{0:0.0}", this.RemainingDuration);
                yield return null;
                this.RemainingDuration -= Time.deltaTime;
                notify -= Time.deltaTime;
                if (notify <= 0) {
                    if (this.OnNotify != null) {
                        this.OnNotify();
                    }
                    notify = this.NotifyDuration;
                }
            }
            this.Label.text = "";

            if (this.OnExpired != null) {
                this.OnExpired();
            }
        }

    }

}