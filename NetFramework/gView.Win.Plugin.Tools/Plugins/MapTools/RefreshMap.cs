using gView.Framework.Carto;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("F4F7F60D-B560-4233-96F7-89012FD856A8")]
    public class RefreshMap : ITool, IShortCut
    {
        IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.RefreshMap", "Refresh Map"); }
        }

        public bool Enabled
        {
            get
            {
                return (_doc != null && _doc.FocusMap != null);
            }
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
            get { return gView.Win.Plugin.Tools.Properties.Resources.Refresh; }
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
            if (_doc != null && _doc.Application is IMapApplication)
            {
                ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
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
    }
}
