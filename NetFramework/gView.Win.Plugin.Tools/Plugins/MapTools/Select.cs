using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Events;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("646860CF-4F82-424b-BF7D-822BE7A214FF")]
    public class Select : gView.Framework.UI.ITool, gView.Framework.UI.IToolWindow
    {
        private gView.Plugins.MapTools.Controls.SelectionEnvironmentControl _dlg;
        private IMapApplication _app = null;
        private SelectionGraphicsElement _element;
        private GraphicsContainer _container;

        public Select()
        {
            _element = new SelectionGraphicsElement();
            _container = new GraphicsContainer();
            _container.Elements.Add(_element);
            _relation = spatialRelation.SpatialRelationIntersects;
        }

        #region ITool Member

        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[7];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument && ((IMapDocument)hook).Application is IMapApplication)
            {
                _dlg = new gView.Plugins.MapTools.Controls.SelectionEnvironmentControl(this);
                _dlg.MapDocument = (IMapDocument)hook;
                _app = ((IMapDocument)hook).Application as IMapApplication;
            }
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Select", "Select");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            MapEvent ev = MapEvent as MapEvent;
            if (ev == null || ev.Map == null || ev.Map.Display == null)
            {
                return Task.FromResult(true);
            }

            if (ev.Map.SelectionEnvironment == null)
            {
                return Task.FromResult(true);
            }

            if (ev is MapEventClick && _methode != gView.Plugins.MapTools.Controls.selectionMothode.Rectangle)
            {
                MapEventClick evClick = (MapEventClick)ev;

                _element.AddPoint(new Point(evClick.x, evClick.y));
                ev.Map.Display.DrawOverlay(_container, true);
                return Task.FromResult(true); ;
                //if ((filter.Geometry = _element.Geometry) == null) return;
                //filter.SpatialReference = ev.map.Display.SpatialReference;
            }
            else
            {
                if (!(ev is MapEventRubberband))
                {
                    return Task.FromResult(true);
                }

                MapEventRubberband evRubberband = (MapEventRubberband)ev;

                IEnvelope envelope = new gView.Framework.Geometry.Envelope(
                    evRubberband.minX, evRubberband.minY,
                    evRubberband.maxX, evRubberband.maxY);

                if (SelectByGeometry(ev.Map, envelope))
                {
                    ev.refreshMap = true;
                    ev.drawPhase = DrawPhase.Selection | DrawPhase.Graphics;
                }
            }

            return Task.FromResult(true);
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return
                    (_methode == gView.Plugins.MapTools.Controls.selectionMothode.Rectangle) ? gView.Framework.UI.ToolType.rubberband : gView.Framework.UI.ToolType.click;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion

        #region IToolWindow Members

        public IDockableWindow ToolWindow
        {
            get { return _dlg; }
        }

        #endregion

        private gView.Plugins.MapTools.Controls.selectionMothode _methode;
        internal gView.Plugins.MapTools.Controls.selectionMothode SelectionMethode
        {
            get { return _methode; }
            set
            {
                _methode = _element.Methode = value;
                _element.ClearPoints();
            }
        }

        private spatialRelation _relation;
        internal spatialRelation SpatialRelation
        {
            get { return _relation; }
            set { _relation = value; }
        }

        private CombinationMethod _combinationMethode = CombinationMethod.New;
        internal CombinationMethod CombinationMethode
        {
            get { return _combinationMethode; }
            set { _combinationMethode = value; }
        }

        internal void ClearSelectionFigure()
        {
            try
            {
                _dlg.MapDocument.FocusMap.Display.ClearOverlay();
                _element.ClearPoints();
            }
            catch { }
        }
        internal IGeometry SelectionGeometry
        {
            get
            {
                return _element.Geometry;
            }
        }
        internal bool SelectByGeometry(IMap map, IGeometry geometry)
        {
            if (map == null || map.Display == null || geometry == null)
            {
                return false;
            }

            SpatialFilter filter = new SpatialFilter();

            filter.Geometry = geometry;
            filter.FilterSpatialReference = map.Display.SpatialReference;
            // FeatureSpatialReference egal, es geht nur um die IDs
            //filter.FeatureSpatialReference = map.Display.SpatialReference;
            filter.SpatialRelation = _relation;

            if (_combinationMethode == CombinationMethod.New)
            {
                map.ClearSelection();
            }

            foreach (IDatasetElement layer in map.SelectionEnvironment.SelectableElements)
            {
                if (!(layer is IFeatureSelection))
                {
                    continue;
                } ((IFeatureSelection)layer).Select(filter, _combinationMethode);
                ((IFeatureSelection)layer).FireSelectionChangedEvent();
            }
            return true;
        }
    }
}
