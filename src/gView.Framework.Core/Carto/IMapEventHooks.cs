using gView.Framework.Core.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.Carto
{
    public interface IMapEventHooks : IPersistable
    {
        IEnumerable<IMapEventHook> EventHooks { get; }

        void Add(IMapEventHook hook);
        void Remove(IMapEventHook hook);
    }

    public enum HookEventType
    {
        OnLoaded = 1
    }

    public interface IMapEventHook : IPersistable
    {
        HookEventType Type { get; }

        Task InvokeAsync(IMap map);
    }
}
