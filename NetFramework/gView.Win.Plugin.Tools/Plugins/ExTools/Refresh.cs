using gView.Framework.Globalisation;
using gView.Framework.UI;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.ExTools
{
    [gView.Framework.system.RegisterPlugIn("54597B19-B37A-4fa2-BA45-8B4137F2910E")]
    public class Refresh : IExTool, IShortCut, IContextMenuItem
    {
        IExplorerApplication _exapp = null;

        #region IExTool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Refresh", "Refresh"); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public object Image
        {
            get { return gView.Win.Plugin.Tools.Properties.Resources.Refresh; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IExplorerApplication)
            {
                _exapp = hook as IExplorerApplication;
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_exapp != null)
            {
                _exapp.RefreshContents();
            }

            return Task.FromResult(true);
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get { return Keys.F5; }
        }

        public string ShortcutKeyDisplayString
        {
            get { return "F5"; }
        }

        #endregion

        #region IContextMenuItem Member

        public bool ShowWith(object context)
        {
            return true;
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 15; }
        }

        #endregion
    }
}
