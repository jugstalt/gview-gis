using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Plugins.Network.Graphic;
using gView.Framework.Data;
using gView.Framework.Network.Algorthm;
using gView.Framework.Geometry;
using gView.Framework.Network;
using System.Threading.Tasks;

namespace gView.Plugins.Network
{
    [RegisterPlugIn("F3DBF95F-F915-4c10-8568-2BC873A745F6")]
    public class Module : IMapApplicationModule
    {
        private IMapDocument _doc = null;
        private FormRoadBook _dlgRoadBook;

        #region IMapApplicationModule Member

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                _dlgRoadBook = new FormRoadBook();
            }
        }

        #endregion

        static public Module GetModule(IMapDocument doc)
        {
            if (doc == null || !(doc.Application is IMapApplication))
                return null;

            return ((IMapApplication)doc.Application).IMapApplicationModule(new Guid("F3DBF95F-F915-4c10-8568-2BC873A745F6")) as Module;
        }

        private INetworkFeatureClass _nfc = null;
        public event EventHandler OnSelectedNetorkFeatureClassChanged = null;
        public INetworkFeatureClass SelectedNetworkFeatureClass
        {
            get { return _nfc; }
            set
            {
                if (_nfc != value)
                {
                    _nfc = value;
                    _startIndex = -1;
                    _endIndex = -1;

                    _weight = null;
                    if (OnSelectedNetorkFeatureClassChanged != null)
                        OnSelectedNetorkFeatureClassChanged(this, new EventArgs());
                }
            }
        }

        private INetworkTracer _tracer = null;
        public INetworkTracer SelectedNetworkTracer
        {
            get { return _tracer; }
            set
            {
                _tracer = value;
            }
        }

        private IGraphWeight _weight = null;
        public IGraphWeight GraphWeight
        {
            get { return _weight; }
            set { _weight = value; }
        }
        private WeightApplying _weightApplying = WeightApplying.Weight;
        public WeightApplying WeightApplying
        {
            get { return _weightApplying; }
            set { _weightApplying = value; }
        }

        private int _startIndex = -1;
        public int StartNodeIndex
        {
            get { return _startIndex; }
            set
            {
                _startIndex = value;
                if (_startIndex >= 0)
                    _startEdgeIndex = -1;
            }
        }
        private int _endIndex = -1;
        public int EndNodeIndex
        {
            get { return _endIndex; }
            set { _endIndex = value; }
        }
        private int _startEdgeIndex = -1;
        public int StartEdgeIndex
        {
            get { return _startEdgeIndex; }
            set
            {
                _startEdgeIndex = value;
                if (_startEdgeIndex >= 0)
                    _startIndex = -1;
            }
        }

        private IPoint _startPoint = null;
        public IPoint StartPoint
        {
            get { return _startPoint; }
            set { _startPoint = value; }
        }
        private IPoint _endPoint = null;
        public IPoint EndPoint
        {
            get { return _endPoint; }
            set { _endPoint = value; }
        }

        public FormRoadBook RoadBookDlg
        {
            get { return _dlgRoadBook; }
        }

        async public Task<List<int>> QueryAllowedNodeIds(double minBufferDist, double maxBufferDist)
        {
            if (_nfc == null || _startPoint == null || _endPoint == null)
                return null;

            Path path = new Path();
            path.AddPoint(_startPoint);
            path.AddPoint(_endPoint);
            Polyline pLine = new Polyline();
            pLine.AddPath(path);

            double bufferDist = Math.Min(Math.Max(path.Length * 0.1, minBufferDist), maxBufferDist);
            IPolygon polygon = pLine.Buffer(bufferDist);
            if (polygon == null)
                return null;

            SpatialFilter filter = new SpatialFilter();
            filter.Geometry = polygon;
            filter.AddField("FDB_NID");

            IFeatureCursor cursor = await _nfc.GetNodeFeatures(filter);
            if (cursor == null)
                return null;

            List<int> allowedNodeIds = new List<int>();
            IFeature feature;
            while ((feature = await cursor.NextFeature()) != null)
            {
                allowedNodeIds.Add(feature.OID);
            }
            return allowedNodeIds;
        }

        public void RemoveAllNetworkGraphicElements(IDisplay display)
        {
            List<IGraphicElement> list = new List<IGraphicElement>();

            foreach (IGraphicElement grElement in display.GraphicsContainer.Elements)
            {
                if (grElement is GraphicNetworkPathEdge)
                    list.Add((GraphicNetworkPathEdge)grElement);
                else if (grElement is GraphicFlagPoint)
                    list.Add((GraphicFlagPoint)grElement);
            }

            foreach (IGraphicElement element in list)
                display.GraphicsContainer.Elements.Remove(element);
        }
        public void RemoveNetworkStartGraphicElement(IDisplay display)
        {
            GraphicStartPoint g = null;
            foreach (IGraphicElement grElement in display.GraphicsContainer.Elements)
            {
                if (grElement is GraphicStartPoint)
                {
                    g = (GraphicStartPoint)grElement;
                    break;
                }
            }
            if (g != null)
                display.GraphicsContainer.Elements.Remove(g);
        }
        public void RemoveNetworkTargetGraphicElement(IDisplay display)
        {
            GraphicTargetPoint g = null;
            foreach (IGraphicElement grElement in display.GraphicsContainer.Elements)
            {
                if (grElement is GraphicTargetPoint)
                {
                    g = (GraphicTargetPoint)grElement;
                    break;
                }
            }
            if (g != null)
                display.GraphicsContainer.Elements.Remove(g);
        }

        async public Task<IFeatureCursor> NetworkPathEdges(Dijkstra.NetworkPath networkPath)
        {
            if (_nfc == null)
                return null;

            RowIDFilter filter = new RowIDFilter(String.Empty);
            foreach (Dijkstra.NetworkPathEdge edge in networkPath)
            {
                filter.IDs.Add(edge.EId);
            }

            return await _nfc.GetEdgeFeatures(filter);
        }
        async public Task<IFeatureCursor> NetworkPathEdges(NetworkEdgeCollectionOutput edgeCollection)
        {
            if (_nfc == null)
                return null;

            RowIDFilter filter = new RowIDFilter(String.Empty);
            foreach (NetworkEdgeOutput edge in edgeCollection)
            {
                filter.IDs.Add(edge.EdgeId);
            }

            return await _nfc.GetEdgeFeatures(filter);
        }

        async public Task<IFeatureCursor> NetworkPathEdges(Dijkstra.Nodes nodes)
        {
            return await NetworkPathEdges(nodes, string.Empty);
        }
        async public Task<IFeatureCursor> NetworkPathEdges(Dijkstra.Nodes nodes, string fields)
        {
            if (_nfc == null)
                return null;

            //StringBuilder sb = new StringBuilder();
            //sb.Append("FDB_OID in (");
            //foreach (Dijkstra.Node node in nodes)
            //{
            //    //if (node.Dist > maxDistance)
            //    //    continue;
            //    if (sb.Length > 12)
            //        sb.Append(",");
            //    sb.Append(node.EId);
            //}
            //sb.Append(")");

            //QueryFilter filter = new QueryFilter();
            //filter.AddField("FDB_OID");
            //filter.AddField("FDB_SHAPE");
            //if (!String.IsNullOrEmpty(fields))
            //{
            //    foreach (string field in fields.Split(','))
            //        filter.AddField(field);
            //}
            //filter.WhereClause = sb.ToString();
            RowIDFilter filter = new RowIDFilter(String.Empty);
            foreach (Dijkstra.Node node in nodes)
                filter.IDs.Add(node.EId);
            if (!String.IsNullOrEmpty(fields))
            {
                foreach (string field in fields.Split(','))
                    filter.AddField(field);
            }

            return await _nfc.GetEdgeFeatures(filter);
        }
    }
}
