using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using System.Windows.Forms;
using gView.Framework.Globalisation;
using gView.Framework.IO;

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
            get { return global::gView.Plugins.Tools.Properties.Resources.Refresh; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IExplorerApplication)
                _exapp = hook as IExplorerApplication;
        }

        public void OnEvent(object MapEvent)
        {
            if (_exapp != null)
                _exapp.RefreshContents();
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
            get { return global::gView.Plugins.Tools.Properties.Resources.netdirectory; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IExplorerApplication)
                _exapp = hook as IExplorerApplication;
        }

        public void OnEvent(object MapEvent)
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
        }

        #endregion
    }
}
