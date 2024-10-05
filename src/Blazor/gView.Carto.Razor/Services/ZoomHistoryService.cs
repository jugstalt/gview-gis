using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Geometry;
using Microsoft.Extensions.Logging;
using gView.Framework.Core.Extensions;

namespace gView.Carto.Razor.Services;

public class ZoomHistoryService : IZoomHistory
{
    private readonly Stack<StackItem> _zoomHistory;
    private readonly ILogger<ZoomHistoryService> _logger;

    public ZoomHistoryService(ILogger<ZoomHistoryService> logger, int maxItems)
    {
        _zoomHistory = new();
        _logger = logger;
    }

    public void Push(IEnvelope bounds)
    {
        if (SuppressOnce)
        {
            _logger.LogInformation("SuppressOnce");

            SuppressOnce = false;
            return;
        }

        // if last item is less than 0.5 seconds ago, remove it
        // maybe its from mousscrolling events, etc
        //if (_zoomHistory.TryPeek(out StackItem? lastItem)
        //    && (DateTime.Now - lastItem.TimeStamp).TotalSeconds < 0.5)
        //{
        //    _logger.Log(LogLevel.Information, "Pop latest item, because its not older than 2 secongs");

        //    _ = _zoomHistory.Pop();
        //}

        _logger.Log(LogLevel.Information, "Push bounds[{count}]: {bounds}", () => _zoomHistory.Count, () => bounds.ToBBoxString());

        _zoomHistory.Push(new StackItem(DateTime.Now, bounds));

        // todo: maxItems ... shrink the stack
    }

    public IEnvelope? Pop()
    {
        if (_zoomHistory.TryPop(out StackItem? lastItem))
        {
            _logger.Log(LogLevel.Information, "Pop item with bounds[{count}]: {bounds}", () => _zoomHistory.Count, () => lastItem.Bounds?.ToBBoxString());

            return lastItem.Bounds;
        }

        return null;
    }

    public void Clear() => _zoomHistory.Clear();   
    

    public bool SuppressOnce { get; set; }

    public bool HasItems => _zoomHistory.Count > 0;

    #region Item Classes

    private record StackItem(DateTime TimeStamp, IEnvelope Bounds);

    #endregion
}
