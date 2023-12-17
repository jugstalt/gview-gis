using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerObjectContextTool
{
    string Name { get; }
    string Icon { get; }

    bool IsEnabled(IExplorerApplicationScopeService scope, IExplorerObject exObject);
    Task<bool> OnEvent(IExplorerApplicationScopeService scope, IExplorerObject exObject);
}
