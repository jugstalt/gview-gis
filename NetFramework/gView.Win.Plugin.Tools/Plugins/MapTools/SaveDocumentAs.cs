using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("17D0A3C1-5EE9-4ddd-9402-E6E9EAB1CD06")]
    public class SaveDocumentAs : gView.Framework.UI.ITool
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public object Image
        {
            get
            {
                return gView.Win.Plugin.Tools.Properties.Resources.save_as_16_w;
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.SaveAs", "Save As...");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_doc == null)
            {
                return Task.FromResult(true);
            }

            if (!(_doc.Application is IMapApplication))
            {
                return Task.FromResult(true);
            }

            System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Map Files (*.mxl)|*.mxl|Reader Files (*.rdm)|*.rdm";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                gView.Plugins.Tools.MapTools.Dialogs.FormSaveEncrypted saveEncDlg = new Tools.MapTools.Dialogs.FormSaveEncrypted();
                if (saveEncDlg.ShowDialog() == DialogResult.OK)
                {
                    ((IMapApplication)_doc.Application).SaveMapDocument(dlg.FileName, saveEncDlg.SaveEncrypted);
                }
            }

            return Task.FromResult(true);
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return ToolType.command;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion

    }
}
