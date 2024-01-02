using gView.Framework.Core.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace gView.Carto.Core.Services;

public class CartoDataTableService
{
    private readonly ConcurrentDictionary<ILayer, object> _layers = new();
    private ILayer? _currentLayer;

    public CartoDataTableService() { }

    public bool AddIfNotExists(ILayer layer, bool setCurrent = true)
    {
        if (_layers.ContainsKey(layer))
        {
            if (_layers.TryAdd(layer, new()))
            {
                return false;
            }
        }

        if (setCurrent)
        {
            _currentLayer = layer;
        }

        return true;
    }

    public ILayer? CurrentLayer => _currentLayer;

    public IEnumerable<ILayer> Layers => _layers.Keys;
}
