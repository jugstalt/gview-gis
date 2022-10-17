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
    [RegisterPlugInAttribute("82F8E9C3-7B75-4633-AB7C-8F9637C2073D")]
    public class ZoomBack : gView.Framework.UI.ITool
    {
        private IMapDocument _doc;

        private void ZoomBack_MapScaleChanged(IDisplay sender)
        {
            if (sender == null)
            {
                return;
            }

            ZoomStack.Push(sender.Envelope);
        }

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.ZoomBack", "Back"); }
        }

        public bool Enabled
        {
            get
            {
                return (ZoomStack.Count > 0);
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
            get { return (new Buttons()).imageList1.Images[12]; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;

                if (_doc is IMapDocumentEvents)
                {
                    ((IMapDocumentEvents)_doc).MapScaleChanged += new MapScaleChangedEvent(ZoomBack_MapScaleChanged);
                }
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_doc == null)
            {
                return Task.FromResult(true);
            }

            ZoomForwardStack.Push(ZoomStack.Pop());
            IEnvelope env = ZoomStack.Pop();
            if (env == null)
            {
                return Task.FromResult(true);
            }

            if (_doc is IMapDocumentEvents)
            {
                ((IMapDocumentEvents)_doc).MapScaleChanged -= new MapScaleChangedEvent(ZoomBack_MapScaleChanged);
            }

            ((MapEvent)MapEvent).Map.Display.ZoomTo(new Envelope(env.minx, env.miny, env.maxx, env.maxy));

            if (_doc is IMapDocumentEvents)
            {
                ((IMapDocumentEvents)_doc).MapScaleChanged += new MapScaleChangedEvent(ZoomBack_MapScaleChanged);
            } ((MapEvent)MapEvent).refreshMap = true;
            ((MapEvent)MapEvent).drawPhase = DrawPhase.All;

            return Task.FromResult(true);
        }

        #endregion
    }
}
