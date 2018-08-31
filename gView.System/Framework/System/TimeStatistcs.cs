using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.system
{
    public class TimeEvent : ITimeEvent
    {
        private string _name;
        private DateTime _start, _finish;
        private int _counter = -1;

        #region ITimeEvent Member

        public TimeEvent(string name, DateTime start, DateTime finish)
        {
            _name = name;
            _start = start;
            _finish = finish;
        }
        public TimeEvent(string name, DateTime start, DateTime finish, int counter)
            : this(name, start, finish)
        {
            _counter = counter;
        }

        public string Name
        {
            get { return _name; }
        }

        public DateTime StartTime
        {
            get { return _start; }
        }

        public DateTime FinishTime
        {
            get { return _finish; }
        }

        public TimeSpan Duration
        {
            get { return _finish-_start; }
        }

        public int Counter
        {
            get { return _counter; }
        }
        #endregion
    }

    public class TimeStatistics : ITimeStatistics
    {
        private List<ITimeEvent> _events;

        public TimeStatistics()
        {
            _events = new List<ITimeEvent>();
        }

        #region ITimeStatistics Member

        public event TimeEventAddedEventHandler TimeEventAdded;
        public event TimeEventsRemovedEventHandler TimeEventsRemoved;

        public void RemoveTimeEvents()
        {
            _events.Clear();
            if (TimeEventsRemoved != null) TimeEventsRemoved(this);
        }

        public void AddTimeEvent(ITimeEvent timeEvent)
        {
            if (timeEvent == null) return;

            _events.Add(timeEvent);
            if (TimeEventAdded != null) TimeEventAdded(this, timeEvent);
        }

        public void AddTimeEvent(string name, DateTime startTime, DateTime finishTime)
        {
            AddTimeEvent(new TimeEvent(name, startTime, finishTime));
        }

        public List<ITimeEvent> TimeEvents
        {
            get { return ListOperations<ITimeEvent>.Clone(_events); }
        }

        #endregion
    }
}
