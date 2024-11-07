using gView.Blazor.Models.DataTable;
using gView.Framework.Core.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace gView.Carto.Core.Services;

public class CartoDataTableService
{
    private readonly ConcurrentDictionary<ILayer, DataTableProperties> _layers = new();
    private ILayer? _currentLayer;

    public CartoDataTableService() { }

    public bool AddIfNotExists(ILayer layer, bool setCurrent = true,
                               DataTableProperties? tableProperties = null)
    {
        if (!_layers.ContainsKey(layer))
        {
            if (!_layers.TryAdd(layer, tableProperties ?? new()))
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

    public bool RemoveLayer(ILayer layer)
        => _layers.TryRemove(layer, out _);

    public DataTableProperties GetProperties(ILayer layer)
        => _layers.ContainsKey(layer)
        ? _layers[layer]
        : new();
}
