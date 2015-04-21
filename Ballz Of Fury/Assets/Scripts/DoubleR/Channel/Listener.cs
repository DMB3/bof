using System;

namespace DoubleR.Channel {

    /// <summary>
    /// Implements a wait for a signal.
    /// Whenever some signal is broadcast with the same type on the same channel, the listener will activate.
    /// If the type is Signal.ANY, the listener will be activated by the next signal to be broadcast in the channel,
    /// independently of its type. In addition to filtering signals by type, listeners may also have a 'condition'
    /// predicate that is able to inspect the contents of matching signals, to further narrow down the scope of the
    /// listener and only activate when the desired signals are emitted.
    /// Listeners also have a 'priority' attribute that defines the order by which listeners are activated when a signal
    /// triggers multiple listeners simultaneously (smaller priority listeners activate first). Listeners with the same
    /// priority will activate in the order of their insertion in the channel (when the Start() method is called).
    /// </summary>
    public class Listener : IComparable<Listener> {

        public delegate void CallbackMethod(Listener listener);
        public delegate bool ConditionPredicate(Signal signal);

        private Channel channel;
        private object type;
        private CallbackMethod callback;
        private ConditionPredicate condition;
        private double priority;
        private object owner;
        private Signal match;
        internal bool deployed;

        /// <summary>
        /// Instantiate a new listener.
        /// </summary>
        /// <param name="channel">The channel where the listener is deployed.</param>
        /// <param name="type">The type of signal that triggers this listener.</param>
        /// <param name="callback">The callback that will be invoked when the listener triggers.</param>
        /// <param name="condition">A condition predicate used to filter matching signals. Defaults to null.</param>
        /// <param name="priority">The listener activation priority. Defaults to 0.</param>
        /// <param name="owner">The owner of this signal. Defaults to null.</param>
        public Listener(Channel channel, object type, CallbackMethod callback, ConditionPredicate condition = null, double priority = 0.0, object owner = null) {
            this.channel = channel;
            this.type = type;
            this.callback = callback;
            this.condition = condition;
            this.priority = priority;
            this.owner = owner;
            this.match = null;
            this.deployed = false;
        }

        public override string ToString() {
            string condition = (this.condition == null) ? "" : String.Format(" if {0}()", this.condition);
            string owner = (this.owner == null) ? "" : String.Format(" by {0}", this.owner);
            return String.Format("|{0}|{1} -> {2}(){3}, priority={4}", this.type, condition, this.callback, owner, this.priority);
        }

        public object Type {
            get {
                return this.type;
            }
            set {
                if (this.deployed) {
                    this.channel.Remove(this);
                    this.type = value;
                    this.channel.Insert(this);
                } else {
                    this.type = value;
                }
            }
        }

        public double Priority {
            get {
                return this.priority;
            }
            set {
                if (this.deployed) {
                    this.channel.Remove(this);
                    this.priority = value;
                    this.channel.Insert(this);
                } else {
                    this.priority = value;
                }
            }
        }

        public bool Deployed {
            get {
                return this.deployed;
            }
            set {
                if (this.deployed && !value) {
                    this.channel.Remove(this);
                } else if (!this.deployed && value) {
                    this.channel.Insert(this);
                }
            }
        }

        public Signal Match {
            get {
                return this.match;
            }
        }

        public object Owner {
            get {
                return owner;
            }
        }

        public Channel Channel {
            get {
                return channel;
            }
        }

        public void Start(bool force = false) {
            if (force || !this.deployed) {
                this.channel.Insert(this);
            }
        }

        public void Stop(bool force = false) {
            if (force || this.deployed) {
                this.channel.Remove(this);
            }
        }

        internal bool Matches(Signal signal) {
            return this.condition == null || this.condition(signal);
        }

        internal void Activate(Signal signal) {
            this.match = signal;
            this.callback(this);
            this.match = null;
        }

        public int CompareTo(Listener listener) {
            return this.priority.CompareTo(listener.priority);
        }

    }

}
