using gView.Framework.Carto;
using gView.Framework.Globalisation;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Events;
using gView.Win.Plugin.Tools.Plugins.MapTools.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Win.Plugin.Tools.Plugins.MapTools
{
    [RegisterPlugIn("78DA3670-8F3D-49B9-A38D-12BADB554E9A")]
    public class MatchGeoserviceLayerIds : ITool , IMapContextMenuItem
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.MatchGeoserviceLayerIds", "Match GeoService Layer Ids..."); }
        }

        public bool Enabled
        {
            get
            {
                return _doc != null && _doc.FocusMap != null;
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
            get { return gView.Win.Plugin.Tools.Properties.Resources.publish; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public Task<bool> OnEvent(object mapEvent)
        {
            if (_doc == null || _doc.FocusMap == null)
            {
                return Task.FromResult(true);
            }

            IMap map = _doc.FocusMap;
            if (mapEvent is MapEvent && ((MapEvent)mapEvent).Map != null)
            {
                map = ((MapEvent)mapEvent).Map;
            }

            var dialog = new FormMatchGeoserviceLayerIds(map);
            dialog.ShowDialog();

            return Task.FromResult(true);
        }

        #endregion

        #region IContextMenuTool Member

        public bool Enable(object element)
        {
            return element is IMap && ((IMap)element).MapElements != null &&
                ((IMap)element).MapElements.Count > 0;
        }

        public bool Visible(object element)
        {
            return true;
        }

        async public Task<bool> OnEvent(object element, object parent)
        {
            if (!(element is IMap))
            {
                return true;
            }

            MapEvent mapEvent = new MapEvent(element as IMap);
            await this.OnEvent(mapEvent);

            return true;
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 201; }
        }

        #endregion
    }
}
