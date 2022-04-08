using System;
using System.Collections.Generic;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.system
{
    public interface ITimeStatistics
    {
        event TimeEventAddedEventHandler TimeEventAdded;
        event TimeEventsRemovedEventHandler TimeEventsRemoved;

        void RemoveTimeEvents();
        void AddTimeEvent(ITimeEvent timeEvent);
        void AddTimeEvent(string name, DateTime startTime, DateTime finishTime);

        List<ITimeEvent> TimeEvents { get; }
    }
}
