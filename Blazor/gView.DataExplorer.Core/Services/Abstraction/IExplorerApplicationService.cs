using gView.Framework.DataExplorer.Abstraction;

namespace gView.DataExplorer.Core.Services.Abstraction;

public interface IExplorerApplicationService
{
    public IExplorerObject RootExplorerObject { get; }
}
