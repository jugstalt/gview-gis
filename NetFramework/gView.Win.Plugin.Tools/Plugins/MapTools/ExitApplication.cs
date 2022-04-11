using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("97F8675C-E01F-451e-AAE0-DC29CD547EB5")]
    public class ExitApplication : ITool, IExTool
    {
        IMapDocument _doc = null;
        IExplorerApplication _exapp = null;

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Exit", "Exit"); }
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
            get
            {
                return gView.Win.Plugin.Tools.Properties.Resources.exit_16_w;
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
            else if (hook is IExplorerApplication)
            {
                _exapp = (IExplorerApplication)hook;
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_doc != null && _doc.Application != null)
            {
                _doc.Application.Exit();
            }
            if (_exapp != null)
            {
                _exapp.Exit();
            }

            return Task.FromResult(true);
        }

        #endregion
    }
}
