using System;
using System.Collections.Generic;

namespace DoubleR.Calendar {

    /// <summary>
    /// Base class for calendar types, providing basic facilities for scheduling events and managing time.
    /// </summary>
    public class Calendar {

        private double time;
        private List<Event> events;

        /// <summary>
        /// Instantiate a new empty calendar.
        /// </summary>
        /// <param name="date">Date (instant) at which the calendar will start. Defaults to 0.</param>
        public Calendar(double date = 0.0) {
            this.time = date;
            this.events = new List<Event>();
        }

        /// <summary>
        /// For debugging purposes, override the default ToString() representation.
        /// </summary>
        /// <returns>A string representation of this Calendar, for debugging purposes.</returns>
        public override string ToString() {
            return String.Format("time={0}, events={1}", this.time, this.events.Count);
        }

        /// <summary>
        /// Reset the calendar to a specified date, wiping all events.
        /// </summary>
        /// <param name="date">Date (instant) at which to reset the calendar. Defaults to 0.</param>
        public void Clear(double date = 0.0) {
            this.time = date;
            this.events.Clear();
        }

        public double Time {
            get {
                return this.time;
            }
            set {
                if (value < this.time) {
                    throw new InvalidOperationException("Invalid attempt to go back in time.");
                }
                while (this.events.Count > 0) {
                    Event next = this.events[0];
                    if (next.Date > value) {
                        break;
                    }
                    this.events.RemoveAt(0);
                    next.deployed = false;
                    next.Activate();
                }
                this.time = value;
            }
        }

        public void Advance(double delta) {
            this.Time += delta;
        }

        public IEnumerable<double> Dates {
            get {
                foreach (Event scheduled in this.events) {
                    yield return scheduled.Date;
                }
            }
        }

        public IEnumerable<Event> Events {
            get {
                foreach (Event scheduled in this.events) {
                    yield return scheduled;
                }
            }
        }

        /// <summary>
        /// Advance 'n' events in the calendar, triggering any events during advancement.
        /// </summary>
        /// <param name="n">The number of events to advance. Defaults to 1.</param>
        /// <returns>The number of events that were triggered.</returns>
        public int Step(int n = 1) {
            int i = 0;
            for (i = 0; i < n; i++) {
                if (this.events.Count == 0) {
                    break;
                }
                Event next = this.events[0];
                this.events.RemoveAt(0);
                this.time = next.Date;
                next.deployed = false;
                next.Activate();
            }
            return i;
        }

        /// <summary>
        /// Advance 'n' instants in the calendar, triggering any events during advancement.
        /// </summary>
        /// <param name="n">The number of instants to advance. Defaults to 1.</param>
        /// <returns>The number of instants that were advanced.</returns>
        public int Jump(int n = 1) {
            int i = 0;
            for (i = 0; i < n; i++) {
                if (this.events.Count == 0) {
                    break;
                }
                this.Time = this.events[0].Date;
            }
            return i;
        }

        /// <summary>
        /// Schedule a new event.
        /// </summary>
        /// <param name="date">The specific date on which to schedule the event.</param>
        /// <param name="callback">The callback method that will be invoked when the event triggers.</param>
        /// <param name="priority">The priority of the event. Lower priority events at the same date trigger first. Defaults to 0.</param>
        /// <param name="owner">The owner (for debugging purposes) of the event.</param>
        /// <returns>The event that was scheduled.</returns>
        public Event ScheduleAt(double date, Event.CallbackMethod callback, double priority = 0.0, object owner = null) {
            Event evt = new Event(this, date, callback, priority: priority, owner: owner);
            evt.Schedule();
            return evt;
        }

        /// <summary>
        /// Schedule an event after a set delta.
        /// </summary>
        /// <param name="delta">The delta after which to schedule the event.</param>
        /// <param name="callback">The callback method that will be invoked when the event triggers.</param>
        /// <param name="priority">The priority of the event. Lower priority events at the same date trigger first. Defaults to 0.</param>
        /// <param name="owner">The owner (for debugging purposes) of the event.</param>
        /// <returns>The event that was scheduled.</returns>
        public Event ScheduleAfter(double delta, Event.CallbackMethod callback, double priority = 0.0, object owner = null) {
            return this.ScheduleAt(this.time + delta, callback, priority: priority, owner: owner);
        }

        internal void Insert(Event evt) {
            if (evt.Deployed) {
                throw new InvalidOperationException("Duplicate attempt to deploy event.");
            }
            if (evt.Date < this.time) {
                throw new InvalidOperationException("Unable to schedule events in the past.");
            }
            this.events.Add(evt);
            this.events.Sort();
            evt.deployed = true;
        }

        internal void Remove(Event evt) {
            if (!evt.Deployed) {
                throw new InvalidOperationException("Event is not deployed.");
            }
            this.events.Remove(evt);
            evt.deployed = false;
        }


    }

}
