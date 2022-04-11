using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Events;
using System;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("CFE66CDF-CD95-463c-8CD1-2541574D719A")]
    public class ZoomForward : gView.Framework.UI.ITool
    {

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.ZoomForward", "Forward"); }
        }

        public bool Enabled
        {
            get
            {
                return (ZoomForwardStack.Count > 0);
            }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return (new Buttons()).imageList1.Images[13]; }
        }

        public void OnCreate(object hook)
        {

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            MapEvent ev = (MapEvent)MapEvent;
            if (ev.Map == null)
            {
                return Task.FromResult(true);
            }

            IEnvelope env = ZoomForwardStack.Pop();
            if (env == null)
            {
                return Task.FromResult(true);
            }

            ev.Map.Display.ZoomTo(new Envelope(env.minx, env.miny, env.maxx, env.maxy));
            ev.refreshMap = true;
            ev.drawPhase = DrawPhase.All;

            return Task.FromResult(true);
        }

        #endregion
    }
}
