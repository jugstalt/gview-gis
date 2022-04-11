using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("7219766E-55AB-4f64-B65E-C2DBC70E5786")]
    public class About : ITool, IExTool
    {
        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.About", "About..."); }
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
            get { return gView.Win.Plugin.Tools.Properties.Resources.help; }
        }

        public void OnCreate(object hook)
        {

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            gView.Framework.system.UI.AboutBox dlg = new gView.Framework.system.UI.AboutBox();
            dlg.ShowDialog();

            return Task.FromResult(true);
        }

        #endregion
    }
}
