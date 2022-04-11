using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("0728E12C-AC12-4264-9B47-ECE6BB0CFFA9")]
    public class TOCCommand : ITool
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.TOC", "TOC"); }
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
            get { return (new Buttons().imageList1.Images[21]); }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = hook as IMapDocument;
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_doc == null || !(_doc.Application is IMapApplication))
            {
                return Task.FromResult(true);
            }

            foreach (IDockableWindow win in ((IMapApplication)_doc.Application).DockableWindows)
            {
                if (win.Name == "TOC")
                {
                    ((IMapApplication)_doc.Application).ShowDockableWindow(win);
                }
            }

            return Task.FromResult(true);
        }

        #endregion
    }
}
