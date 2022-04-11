using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Events;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("3E2E9F8C-24FB-48f6-B80E-1B0A54E8C309")]
    public class SmartNavigation : gView.Framework.UI.ITool, IScreenTip
    {
        public SmartNavigation()
        {

        }
        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.SmartNavigation", "Zoom/Pan");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return gView.Framework.UI.ToolType.smartnavigation;
            }
        }

        public void OnCreate(object hook)
        {

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (MapEvent is MapEventRubberband)
            {

                MapEventRubberband ev = (MapEventRubberband)MapEvent;
                if (ev.Map == null)
                {
                    return Task.FromResult(true);
                }

                if (!(ev.Map.Display is Display))
                {
                    return Task.FromResult(true);
                }

                Display nav = (Display)ev.Map.Display;

                nav.ZoomTo(new Envelope(ev.minX, ev.minY, ev.maxX, ev.maxY));
                ev.refreshMap = true;
            }
            else if (MapEvent is MapEventPan)
            {
                MapEventPan ev = (MapEventPan)MapEvent;
                if (ev.Map == null)
                {
                    return Task.FromResult(true);
                }

                if (!(ev.Map.Display is Display))
                {
                    return Task.FromResult(true);
                }

                Display nav = (Display)ev.Map.Display;

                nav.Pan(ev.dX, ev.dY);
                ev.refreshMap = true;

                return Task.FromResult(true);
            }

            return Task.FromResult(true);
        }

        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[18];
            }
        }
        #endregion

        #region IScreenTip Member

        public string ScreenTip
        {
            get { return "Use left mousebutton for panning.\nUse right mousebutton an move up an down for zooming.\nThis tool is always used, if you press and hold Crtl-Key..."; }
        }

        #endregion
    }
}
