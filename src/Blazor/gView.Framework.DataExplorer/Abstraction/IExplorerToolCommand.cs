using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction
{
    public interface IExplorerToolCommand
    {
        string Name { get; }
        string ToolTip { get; }
        string Icon { get; }
        Task<bool> OnEvent(IExplorerApplicationScopeService scope);
    }
}
