namespace gView.Framework.Core.Common
{
    public delegate void TimeEventAddedEventHandler(ITimeStatistics sender, ITimeEvent timeEvent);
    public delegate void TimeEventsRemovedEventHandler(ITimeStatistics sender);
}
