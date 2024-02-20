using gView.Framework.DataExplorer.Services.Abstraction;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerRootObject : IExplorerObject
{
    IExplorerApplicationScopeService Scope { get; }
    
    string? FileFilter { get; }
}
