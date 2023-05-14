using gView.DataExplorer.Plugins.Extensions;
using gView.Framework.DataExplorer;
using gView.Framework.DataExplorer.Abstraction;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTools
{
    [gView.Framework.system.RegisterPlugIn("4F54D455-1C22-469e-9DBB-78DBBEF6078D")]
    public class Delete : IExplorerTool
    {
        public string Name => "Delete";

        public string ToolTip => "";

        public string Icon => "basic:trashcan";

        public ExplorerToolTarget Target => ExplorerToolTarget.SelectedContextExplorerObjects;

        public bool IsEnabled(IExplorerApplicationScope scope)
        {
            var scopeService = scope.ToScopeService();

            return scopeService.ContextExplorerObjects?
                .Where(e => e is IExplorerObjectDeletable)
                .Count() > 0;
        }

        public Task<bool> OnEvent(IExplorerApplicationScope scope)
        {
            return Task.FromResult(true);
        }

        #region IDisposable

        public void Dispose()
        {

        }

        #endregion

        #region IOrder

        public int SortOrder => 25;

        #endregion
    }
}
