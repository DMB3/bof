using System;

namespace DoubleR.Calendar {

    /// <summary>
    /// An event executes a given callback function when its date is reached in the calendar.
    /// Events should be created through the Calendar.ScheduleAt method.
    /// </summary>
    public class Event : IComparable<Event> {

        public delegate void CallbackMethod();

        private Calendar calendar;
        private double date;
        private CallbackMethod callback;
        private double priority;
        private object owner;
        internal bool deployed;

        /// <summary>
        /// Instantiate a new event.
        /// This does not schedule the event in the calendar.
        /// </summary>
        /// <param name="calendar">The calendar that will be associated with this event.</param>
        /// <param name="date">The date (instant) at which this event will trigger.</param>
        /// <param name="callback">The callback method that will be executed when this event triggers.</param>
        /// <param name="priority">For breaking ties, lower priority events activate first. This value defaults to 0.</param>
        /// <param name="owner">An owner object that may be helpful for debugging.</param>
        public Event(Calendar calendar, double date, CallbackMethod callback, double priority = 0.0, object owner = null) {
            this.calendar = calendar;
            this.date = date;
            this.callback = callback;
            this.priority = priority;
            this.owner = owner;
            this.deployed = false;
        }

        /// <summary>
        /// For debugging purposes, override the default ToString() representation.
        /// </summary>
        /// <returns>A string representation of this Event, for debugging purposes.</returns>
        public override string ToString() {
            string owner = (this.owner == null) ? "" : String.Format(" by {0}", this.owner);
            return String.Format("t={0} -> {1}(){2}, priority={3}", this.date, this.callback, owner, this.priority);
        }

        public Calendar Calendar {
            get {
                return calendar;
            }
        }

        public double Date {
            get {
                return this.date;
            }
            set {
                if (this.deployed) {
                    this.calendar.Remove(this);
                    this.date = value;
                    this.calendar.Insert(this);
                } else {
                    this.date = value;
                }
            }
        }

        public double Priority {
            get {
                return this.priority;
            }
            set {
                if (this.deployed) {
                    this.calendar.Remove(this);
                    this.priority = value;
                    this.calendar.Insert(this);
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
                    this.calendar.Remove(this);
                } else if (!this.deployed && value) {
                    this.calendar.Insert(this);
                }
            }
        }

        public void Schedule() {
            this.calendar.Insert(this);
        }

        public void Cancel() {
            this.calendar.Remove(this);
        }

        internal void Activate() {
            this.callback();
        }

        public int CompareTo(Event evt) {
            int dateComp = this.date.CompareTo(evt.date);
            if (dateComp == 0) {
                return this.priority.CompareTo(evt.priority);
            }
            return dateComp;
        }

    }

}
