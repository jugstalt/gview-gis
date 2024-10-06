using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Geometry;
using Microsoft.Extensions.Logging;
using gView.Framework.Core.Extensions;
using gView.Framework.Common;

namespace gView.Carto.Razor.Services;

public class ZoomHistoryService : IZoomHistory
{
    private readonly LimitedSizeStack<StackItem> _zoomHistory;
    private readonly ILogger<ZoomHistoryService> _logger;

    public ZoomHistoryService(ILogger<ZoomHistoryService> logger, int maxItems)
    {
        _zoomHistory = new(maxItems);
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

        _logger.Log(LogLevel.Information, "Push bounds[{count}]: {bounds}", () => _zoomHistory.Count, () => bounds.ToBBoxString());

        _zoomHistory.Push(new StackItem(DateTime.Now, bounds));
    }

    public IEnvelope? Pop()
    {
        if (_zoomHistory.TryPop(out StackItem? lastItem))
        {
            _logger.Log(LogLevel.Information, "Pop item with bounds[{count}]: {bounds}", () => _zoomHistory.Count, () => lastItem.Bounds?.ToBBoxString());

            while (true)
            {
                var prevItem = _zoomHistory.Peek();
                if (prevItem is not null && lastItem is not null
                    && (lastItem.TimeStamp - prevItem.TimeStamp).TotalSeconds < 1)
                {
                    lastItem = _zoomHistory.Pop();
                } 
                else
                {
                    break;
                }
            }

            return lastItem?.Bounds;
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
