using System;

namespace DoubleR.Channel {

    /// <summary>
    /// Signals are an instantaneous primitive that allows synchronizing several objects listening on the same channel.
    /// Signals activate listeners, which wait until a signal of matching type is emitted on the same channel.
    /// </summary>
    public class Signal {

        public const string ANY = "*";

        private Channel channel;
        private object type;
        internal object payload;
        internal object owner;

        /// <summary>
        /// Instantiate a new signal.
        /// </summary>
        /// <param name="channel">The channel to which the signal is associated.</param>
        /// <param name="type">The signal type, which cannot be Signal.ANY.</param>
        /// <param name="payload">The payload object that will be attached to the signal.</param>
        /// <param name="owner">The owner of this signal.</param>
        public Signal(Channel channel, object type, object payload = null, object owner = null) {
            this.channel = channel;
            this.type = type;
            this.payload = payload;
            this.owner = owner;
        }

        public override string ToString() {
            string owner = (this.owner == null) ? "" : String.Format("{0}", this.owner);
            string payload = (this.payload == null) ? "" : String.Format("{0}", this.payload);
            return String.Format("|{0}|{1}{2}", this.type, owner, payload);
        }

        public void Emit() {
            this.channel.Broadcast(this);
        }

        public object Payload {
            get {
                return this.payload;
            }
        }

        public object Type {
            get {
                return this.type;
            }
        }

        public object Owner {
            get {
                return this.owner;
            }
        }

    }

}
