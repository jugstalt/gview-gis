using System.Threading.Tasks;
using System;

namespace gView.DataExplorer.Plugins.Abstraction;

public interface IExplorerObject : IDisposable, ISerializableExplorerObject
{
    string Name { get; }
    string FullName { get; }
    string Type { get; }

    IExplorerIcon Icon { get; }
    IExplorerObject ParentExplorerObject { get; }

    Task<object> GetInstanceAsync();
    Type ObjectType { get; }

    int Priority { get; }
}
