using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("CEACE261-ECE4-4622-A892-58A5B32E5295")]
    public class LoadDocument : gView.Framework.UI.ITool, IShortCut
    {
        private MapDocument _doc = null;

        #region ITool Member

        public object Image
        {
            get
            {
                Buttons dlg = new Buttons();
                return dlg.imageList1.Images[0];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is MapDocument)
            {
                _doc = (MapDocument)hook;
            }
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Load", "Load...");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        async public Task<bool> OnEvent(object MapEvent)
        {
            if (_doc == null)
            {
                return true;
            }

            System.Windows.Forms.OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Map Files (*.mxl)|*.mxl|Reader Files (*.rdm)|*.rdm|ArcXml Files (*.axl)|*.axl";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (_doc.Application is IMapApplication)
                {
                    FileInfo fi = new FileInfo(dlg.FileName);
                    FileInfo restoreFi = new FileInfo(dlg.FileName + ".restore");

                    if (restoreFi.Exists && restoreFi.LastWriteTimeUtc > fi.LastWriteTimeUtc)
                    {
                        StringBuilder msg = new StringBuilder();
                        msg.Append("For the specified file there is a 'restore' file that is newer than the actual project file. This may indicate that the project did not finish properly before the program was last closed. The program may have crashed last time!");
                        msg.Append(Environment.NewLine);
                        msg.Append(Environment.NewLine);
                        msg.Append("Note: if you no longer want to show this message, open the project and then save it again.");
                        msg.Append(Environment.NewLine);
                        msg.Append(Environment.NewLine);
                        msg.Append("Do you want to load the 'restore' file instead of the project?");

                        if (MessageBox.Show(msg.ToString(), "Load restore file?", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            await ((IMapApplication)_doc.Application).LoadMapDocument(restoreFi.FullName);
                            return true;
                        }
                    }

                    await ((IMapApplication)_doc.Application).LoadMapDocument(dlg.FileName);
                }
            }

            return true;
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return ToolType.command; ;
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
                return Keys.Control | Keys.O;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+O";
            }
        }

        #endregion

    }
}
