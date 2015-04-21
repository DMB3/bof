using System;
using System.Collections.Generic;
using System.Linq;

using DoubleR.Utils;

namespace DoubleR.Channel {

    /// <summary>
    /// This class implements a channel for asynchronous communication using Signal and Listener primitives.
    /// The primitive classes may be used indirectly; the Listen() and Emit() methods of the Channel object are preferred.
    /// A hierarchy of channels can be set up by linking channels through their 'parent'/'children' attributes. Whenever a signal
    /// is emitted in a channel, after activating the local listeners, the channel will emit the same signal on its parent channel, 
    /// if a parent channel is available.
    /// </summary>
    public class Channel {

        /// <summary>
        /// Inner class that represents a disposable object for signalling with 'using' constructs.
        /// </summary>
        internal class DisposableSignals : IDisposable {

            private Signal initial;
            private Signal final;

            internal DisposableSignals(Signal initial, Signal final) {
                this.initial = initial;
                this.final = final;
            }

            internal DisposableSignals Begin() {
                this.initial.Emit();
                return this;
            }

            internal DisposableSignals End() {
                this.final.Emit();
                return this;
            }

            public void Dispose() {
                this.End();
            }

        }

        public const char NAME_SEPARATOR = '.';

        private string name;
        private string fullName;
        private Channel parent;
        private Dictionary<string, Channel> children;
        private bool typeValidation;
        private Multiset<object> registeredTypes;
        private SortedMultimap<object, Listener> listeners;

        /// <summary>
        /// Instantiate a new channel.
        /// </summary>
        /// <param name="name">The name of this channel. If the channel's name contains the name separator character, an exception is raised.</param>
        /// <param name="parent">The channel that is the parent to this channel. Defaults to null.</param>
        /// <param name="typeValidation">If this is true, then only registered signal types will be accepted by the channel. Defaults to true.</param>
        public Channel(string name, Channel parent = null, bool typeValidation = true) {
            if (name.Contains(Channel.NAME_SEPARATOR)) {
                throw new InvalidOperationException(String.Format("Channel name cannot contain {0}", Channel.NAME_SEPARATOR));
            }
            this.name = name;
            this.fullName = name;
            this.parent = null;
            this.children = new Dictionary<string, Channel>();
            this.typeValidation = typeValidation;
            this.registeredTypes = new Multiset<object>();
            this.listeners = new SortedMultimap<object, Listener>();
            if (parent != null) {
                this.Parent = parent;
            }
        }

        public override string ToString() {
            HashSet<object> signalTypes = new HashSet<object>();
            if (this.typeValidation) {
                signalTypes = new HashSet<object>(this.registeredTypes.KeySet);
            } else {
                signalTypes = new HashSet<object>(this.listeners.KeySet);
            }
            List<string> listenerData = new List<string>();
            foreach (object signalType in signalTypes) {
                int count = this.listeners[signalType].Count();
                listenerData.Add(String.Format("{0} ({1})", signalType, count));
            }
            return String.Format("{0}, typeValidation={1}, {2}", this.fullName, this.typeValidation, listenerData);
        }

        /// <summary>
        /// Register new signal types in the channel. If type validation is enabled, a channel will not accept signals or listeners
        /// of a given type before it is registered.
        /// </summary>
        /// <param name="types">The signal types to register.</param>
        public void Register(params object[] types) {
            RegisterPropagate(types);
        }

        /// <summary>
        /// Register signal types in this channel and propagate upwards.
        /// </summary>
        /// <param name="types">The signal types to register.</param>
        internal void RegisterPropagate(params object[] types) {
            Channel channel = this;
            while (channel != null) {
                channel.registeredTypes.Add(types);
                channel = channel.parent;
            }
        }

        /// <summary>
        /// Remove (if present) one or more given signal types from the channel.
        /// </summary>
        /// <param name="types">The signal types to unregister.</param>
        public void Unregister(params object[] types) {
            UnregisterPropagate(types);
        }

        /// <summary>
        /// Unregisters signal types in 'types' in this channel and propagates upwards.
        /// </summary>
        /// <param name="types">The signal types to unregister.</param>
        internal void UnregisterPropagate(params object[] types) {
            Channel channel = this;
            while (channel != null) {
                channel.registeredTypes.Remove(types);
                channel = channel.parent;
            }
        }

        /// <summary>
        /// Creates a Listener object and attaches it to the channel. Returns the new listener.
        /// Use a type of Signal.ANY to activate on any signal emitted on the channel.
        /// </summary>
        /// <param name="type">The signal type to listen to.</param>
        /// <param name="callback">The callback method that will be invoked.</param>
        /// <param name="condition">The condition predicate to further narrow down the activation. Defaults to null.</param>
        /// <param name="priority">The priority of the listener. Defaults to 0.</param>
        /// <param name="owner">The owner of the listener. Defaults to null.</param>
        /// <returns>The listener that was created.</returns>
        public Listener Listen(object type, Listener.CallbackMethod callback, Listener.ConditionPredicate condition = null, double priority = 0.0, object owner = null) {
            Listener listener = new Listener(this, type, callback, condition: condition, priority: priority, owner: owner);
            listener.Start();
            return listener;
        }

        /// <summary>
        /// Emits a disposable signal that can be used in 'using' constructs.
        /// </summary>
        /// <param name="initialType">The type of the signal that will be emitted first. Signal.ANY is forbidden.</param>
        /// <param name="finalType">The type of signal that will be emitted last. Signal.ANY is forbidden.</param>
        /// <param name="initialPayload">The payload to attach to the initial signal. Defaults to null.</param>
        /// <param name="finalPayload">The payload to attach to the final signa. Defaults to null.</param>
        /// <param name="initialOwner">The owner of the initial signal. Defaults to null.</param>
        /// <param name="finalOwner">The owner of the final signal. Defaults to null.</param>
        /// <returns>A disposable object for 'using' constructs.</returns>
        public IDisposable EmitDisposable(object initialType, object finalType, object initialPayload = null, object finalPayload = null, object initialOwner = null, object finalOwner = null) {
            if (initialType.Equals(Signal.ANY) || finalType.Equals(Signal.ANY)) {
                throw new InvalidOperationException(String.Format("Cannot emit a signal of type ANY ({0}).", Signal.ANY));
            }
            Signal initial = new Signal(this, initialType, payload: initialPayload, owner: initialOwner);
            Signal final = new Signal(this, finalType, payload: finalPayload, owner: finalOwner);
            return new DisposableSignals(initial, final).Begin();
        }

        /// <summary>
        /// Creates a Signal object and broadcasts it on the channel. Returns the new signal.
        /// </summary>
        /// <param name="type">The type of signal to emit. Signal.ANY is forbidden.</param>
        /// <param name="payload">The payload to attach to the signal. Defaults to null.</param>
        /// <param name="owner">The owner of this signal. Defaults to null.</param>
        /// <returns>The signal that was created.</returns>
        public Signal Emit(object type, object payload = null, object owner = null) {
            if (type.Equals(Signal.ANY)) {
                throw new InvalidOperationException(String.Format("Cannot emit a signal of type ANY ({0}).", Signal.ANY));
            }
            Signal signal = new Signal(this, type, payload: payload, owner: owner);
            signal.Emit();
            return signal;
        }

        /// <summary>
        /// Creates a copy of a signal object and broadcasts it on the channel. Returns the new signal.
        /// </summary>
        /// <param name="signal">The signal that will be the basis of the new signal.</param>
        /// <param name="payload">The payload to attach to the new signal. Defaults to null. If null is provided, the original signal's payload will be used.</param>
        /// <param name="owner">The owner of this signal. Defaults to null. If null is provided, the original signal's owner will be used.</param>
        /// <returns>The new signal that was created.</returns>
        public Signal Emit(Signal signal, object payload = null, object owner = null) {
            Signal newSignal = new Signal(this, signal.Type, signal.Payload, signal.Owner);
            if (payload != null) {
                newSignal.payload = payload;
            }
            if (owner != null) {
                newSignal.owner = owner;
            }
            newSignal.Emit();
            return newSignal;
        }

        internal void Insert(Listener listener) {
            if (listener.deployed) {
                throw new InvalidOperationException("Duplicate attempt to deploy listener.");
            }
            if (this.typeValidation && !listener.Type.Equals(Signal.ANY) && !this.registeredTypes.ContainsKey(listener.Type)) {
                throw new InvalidOperationException(String.Format("Unable to listen type {0} (not registered).", listener.Type));
            }
            this.listeners.Add(listener.Type, listener);
            listener.deployed = true;
        }

        internal void Remove(Listener listener) {
            if (!listener.deployed) {
                throw new InvalidOperationException("Listener is not deployed.");
            }
            this.listeners.Remove(listener.Type, listener);
            listener.deployed = false;
        }

        /// <summary>
        /// Find and activate all listeners attached to this channel which match 'signal'.
        /// If the channel is linked to a parent channel, the broadcast is propagated to the parent channel afterwards.
        /// </summary>
        /// <param name="signal">The signal to broadcast.</param>
        internal void Broadcast(Signal signal) {
            if (this.typeValidation && !this.registeredTypes.ContainsKey(signal.Type)) {
                throw new InvalidOperationException(String.Format("Unable to emit signal type {0} (not registered).", signal.Type));
            }
            foreach (object type in new object[] { signal.Type, Signal.ANY }) {
                IEnumerable<Listener> listeners = this.listeners[type];
                foreach (Listener listener in listeners.ToArray()) {
                    if (listener.Matches(signal)) {
                        listener.Activate(signal);
                    }
                }
            }
            if (this.parent != null) {
                this.parent.Broadcast(signal);
            }
        }

        public Channel Root {
            get {
                Channel channel = this;
                while (channel.parent != null) {
                    channel = channel.parent;
                }
                return channel;
            }
        }

        public Channel Parent {
            get {
                return this.parent;
            }
            set {
                if (this.parent != null && this.parent != value) {
                    this.parent.Remove(this);
                }
                this.parent = value;
                if (this.parent != null) {
                    this.parent.Add(this);
                }
                UpdateFullName();
            }
        }

        public IEnumerable<Channel> Children {
            get {
                foreach (Channel child in this.children.Values) {
                    yield return child;
                }
            }
        }

        public void Add(Channel child) {
            if (this.children.ContainsKey(child.name)) {
                throw new InvalidOperationException(String.Format("Name {0} already in use", child.name));
            }
            this.children.Add(child.name, child);
            child.parent = this;
            foreach (object type in child.registeredTypes.KeySet) {
                RegisterPropagate(type);
            }
        }

        public void Remove(Channel child) {
            this.children.Remove(child.name);
            child.parent = null;
            foreach (object type in child.registeredTypes.KeySet) {
                UnregisterPropagate(type);
            }
        }

        public bool Contains(Channel child) {
            if (!this.children.ContainsKey(child.name)) {
                return false;
            }
            return child == this.children[child.name];
        }

        public bool Contains(string childName) {
            return this.children.ContainsKey(childName);
        }

        public string Name {
            get {
                return this.name;
            }
            set {
                if (value.Contains(Channel.NAME_SEPARATOR)) {
                    throw new InvalidOperationException(String.Format("Channel name cannot contain {0}", Channel.NAME_SEPARATOR));
                }
                if (this.parent != null) {
                    this.parent.RenameChild(this, value);
                }
                this.name = value;
                UpdateFullName();
            }
        }

        public string FullName {
            get {
                return fullName;
            }
        }

        private void UpdateFullName() {
            if (this.parent == null) {
                this.fullName = this.name;
            } else {
                this.fullName = String.Format("{0}{1}{2}", this.parent.fullName, Channel.NAME_SEPARATOR, this.name);
            }
            foreach (Channel child in this.children.Values) {
                child.UpdateFullName();
            }
        }

        private void RenameChild(Channel child, string name) {
            if (this.children.ContainsKey(name)) {
                throw new InvalidOperationException(String.Format("Name {0} already in use.", name));
            }
            this.children.Remove(child.name);
            this.children.Add(name, child);
        }

        public bool IsRegistered(object type) {
            return registeredTypes.ContainsKey(type);
        }

        public bool AreRegistered(params object[] types) {
            foreach (object type in types) {
                if (!IsRegistered(type)) {
                    return false;
                }
            }
            return true;
        }

    }

}
