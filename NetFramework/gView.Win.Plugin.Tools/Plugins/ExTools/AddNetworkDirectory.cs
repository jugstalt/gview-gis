using gView.Framework.Globalisation;
using gView.Framework.IO;
using gView.Framework.UI;
using System;
using System.Threading.Tasks;

namespace gView.Plugins.ExTools
{
    [gView.Framework.system.RegisterPlugIn("7cbbed5e-c071-46de-b30a-9c6140dafd75")]
    public class AddNetworkDirectory : IExTool
    {
        IExplorerApplication _exapp = null;

        #region IExTool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("String.MapNetFolder", "Map (network) folder..."); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public object Image
        {
            get { return gView.Win.Plugin.Tools.Properties.Resources.netdirectory; }
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
            gView.Framework.UI.Dialogs.FormAddNetworkDirectory dlg = new gView.Framework.UI.Dialogs.FormAddNetworkDirectory();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && !String.IsNullOrEmpty(dlg.Path))
            {
                ConfigConnections connStream = new ConfigConnections("directories");
                connStream.Add(dlg.Path, dlg.Path);

                if (_exapp != null)
                {
                    //_tree.SelectRootNode();
                    _exapp.RefreshContents();
                }
            }

            return Task.FromResult(true);
        }

        #endregion
    }
}
