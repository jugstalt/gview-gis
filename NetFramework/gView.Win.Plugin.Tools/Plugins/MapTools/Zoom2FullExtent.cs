using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Events;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("58AE3C1D-40CD-4f61-8C5C-0A955C010CF4")]
    public class Zoom2FullExtent : ITool
    {
        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ZoomToFullExtent", "Zoom To Full Extent");
            }
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
            get { return gView.Win.Plugin.Tools.Properties.Resources.map16; }
        }

        public void OnCreate(object hook)
        {

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEvent))
            {
                return Task.FromResult(true);
            }

            MapEvent ev = (MapEvent)MapEvent;
            if (ev.Map == null || ev.Map.Display == null)
            {
                return Task.FromResult(true);
            }

            ev.Map.Display.ZoomTo(ev.Map.Display.Limit);

            ev.refreshMap = true;

            return Task.FromResult(true);
        }

        #endregion
    }
}
