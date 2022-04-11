using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Plugins.MapTools.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("ED5B0B59-2F5D-4b1a-BAD2-3CABEF073A6A")]
    public class Find : gView.Framework.UI.ITool, IShortCut
    {
        private IMapDocument _doc = null;
        private FormIdentify _dlgIdentify = null;
        private FormQuery _dlgQuery = null;

        public QueryThemeMode ThemeMode
        {
            set
            {
                if (_dlgQuery != null)
                {
                    _dlgQuery.ThemeMode = value;
                }
            }
        }

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Find", "Find..."); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return gView.Win.Plugin.Tools.Properties.Resources.find; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                if (_doc != null && _doc.Application is IGUIApplication)
                {
                    Identify identify = ((IGUIApplication)_doc.Application).Tool(new Guid("F13D5923-70C8-4c6b-9372-0760D3A8C08C")) as Identify;
                    if (identify != null)
                    {
                        _dlgIdentify = identify.ToolWindow as FormIdentify;
                    }
                }
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_dlgIdentify != null && _doc != null && _doc.Application is IMapApplication)
            {
                ((IMapApplication)_doc.Application).AddDockableWindow(_dlgIdentify, DockWindowState.right);
                ((IMapApplication)_doc.Application).ShowDockableWindow(_dlgIdentify);

                if (_dlgQuery == null)
                {
                    _dlgQuery = new FormQuery(_dlgIdentify);
                    _dlgQuery.Show(((IGUIApplication)_doc.Application).ApplicationWindow);
                }
                _dlgQuery.Visible = true;
                _dlgQuery.BringToFront();
            }

            return Task.FromResult(true);
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.F;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+F";
            }
        }

        #endregion
    }
}
