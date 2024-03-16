#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.IO;
using System.Collections.Generic;

namespace gView.Framework.Cartography;

public class MapEventHooks : IMapEventHooks
{
    private List<IMapEventHook> _eventHooks = new();

    public IEnumerable<IMapEventHook> EventHooks => _eventHooks.ToArray();

    public void Add(IMapEventHook hook)
    {
        _eventHooks.Add(hook);
    }

    public void Remove(IMapEventHook hook)
    {
        _eventHooks.Remove(hook);
    }

    #region IPersistable

    public void Load(IPersistStream stream)
    {
        _eventHooks.Clear();

        IMapEventHook? hook;
        while ((hook = stream.Load("hook", null) as IMapEventHook) != null)
        {
            _eventHooks.Add(hook);
        }
    }

    public void Save(IPersistStream stream)
    {
        foreach(var hook in EventHooks)
        {
            stream.Save("hook", hook);
        }
    }

    #endregion
}
