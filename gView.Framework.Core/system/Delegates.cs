namespace gView.Framework.Core.system
{
    public delegate void TimeEventAddedEventHandler(ITimeStatistics sender, ITimeEvent timeEvent);
    public delegate void TimeEventsRemovedEventHandler(ITimeStatistics sender);
}
