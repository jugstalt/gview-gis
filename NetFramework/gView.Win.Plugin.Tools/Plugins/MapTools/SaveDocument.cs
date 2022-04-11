using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("FCA2C303-A0B6-4f36-BD21-E1C119EB9C8E")]
    public class SaveDocument : gView.Framework.UI.ITool, IShortCut
    {
        private IMapDocument _doc = null;

        #region ITool Member

        virtual public object Image
        {
            get
            {
                Buttons dlg = new Buttons();
                return dlg.imageList1.Images[1];
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
                return LocalizedResources.GetResString("Tools.Save", "Save");
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

            try
            {
                FileInfo fi = new FileInfo(((IMapApplication)_doc.Application).DocumentFilename);
                if (fi.Name.ToLower() == "newdocument.mxl" || fi.Extension.ToLower() == ".axl")
                {
                    System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "Map Files (*.mxl)|*.mxl|Reader Files (*.rdm)|*.rdm";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        ((IMapApplication)_doc.Application).SaveMapDocument(dlg.FileName, true);
                    }
                }
                else
                {
                    ((IMapApplication)_doc.Application).SaveMapDocument();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.S;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+S";
            }
        }

        #endregion

    }
}
