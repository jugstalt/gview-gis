using gView.Framework.Carto;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI.Events;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("2680F0FD-31EE-48c1-B0F7-6674BAD0A688")]
    public class Pan : gView.Framework.UI.ITool
    {
        public Pan()
        {
        }
        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Pan", "Pan");
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
                return gView.Framework.UI.ToolType.pan;
            }
        }

        public void OnCreate(object hook)
        {

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEventPan))
            {
                return Task.FromResult(true);
            }

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
        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[6];
            }
        }
        #endregion
    }
}
