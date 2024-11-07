using gView.Framework.Core.UI;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction
{
    public interface IExplorerTool : IOrder, IDisposable
    {
        string Name { get; }

        bool IsEnabled(IExplorerApplicationScopeService scope);

        string ToolTip { get; }

        string Icon { get; }

        ExplorerToolTarget Target { get; }

        Task<bool> OnEvent(IExplorerApplicationScopeService scope);
    }
}
