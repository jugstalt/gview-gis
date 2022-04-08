using System;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.system
{
    public interface ITimeEvent
    {
        string Name { get; }
        DateTime StartTime { get; }
        DateTime FinishTime { get; }
        TimeSpan Duration { get; }
        int Counter { get; }
    }
}
