using gView.Framework.Cartography.Rendering.UI;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.Common;
using gView.Framework.Core.UI;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using gView.Framework.Symbology;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using gView.Framework.Common.Collection;

namespace gView.Framework.Cartography.Rendering
{
    [RegisterPlugIn("0be572ae-bb58-4148-a428-084bad28ffab")]
    public class ChartLabelRenderer : Cloner, ILabelRenderer, IPriority, IDefault, ILegendGroup
    {
        public enum RenderChartType { Pie = 0, Bars = 1, Stack = 2 }
        public enum RenderSizeType { ConstantSize = 0, ValueOfEquatesToSize = 1 }

        private RenderChartType _type = RenderChartType.Pie;
        private RenderSizeType _sizeType = RenderSizeType.ConstantSize;
        private SimpleLabelRenderer.RenderLabelPriority _labelPriority = SimpleLabelRenderer.RenderLabelPriority.Normal;
        private OrderedKeyValuePairs<string, ISymbol> _symbolTable = new();
        private ILineSymbol _outlineSymbol = null;
        private double _size = 50D, _valueEquatesToSize = 100D;
        private IEnvelope _clipEnvelope = null;

        public ChartLabelRenderer()
        {
        }

        #region Members / Properties

        public void SetSymbol(string fieldName, ISymbol symbol)
        {
            if (symbol == null)
            {
                if (_symbolTable.ContainsKey(fieldName))
                {
                    _symbolTable[fieldName]?.Release();
                    _symbolTable.Remove(fieldName);
                }
            }
            else
            {
                if (!_symbolTable.ContainsKey(fieldName))
                {
                    _symbolTable.Add(fieldName, symbol);
                }
                else
                {
                    _symbolTable[fieldName].Release();
                    _symbolTable[fieldName] = symbol;
                }
            }
        }

        public void RemoveAllSymbols()
        {
            foreach(var symbol in _symbolTable.Values.ToArray()) 
            { 
                symbol?.Release(); 
            }

            _symbolTable.Clear(); 
        }

        public ISymbol GetSymbol(string fieldName)
        {
            if (_symbolTable.ContainsKey(fieldName))
            {
                return _symbolTable[fieldName];
            }
            return null;
        }

        public string[] FieldNames
        {
            get
            {
                return _symbolTable.Keys.ToArray();
            }
        }

        public ILineSymbol OutlineSymbol
        {
            get { return _outlineSymbol; }
            set { _outlineSymbol = value; }
        }

        public double Size
        {
            get { return _size; }
            set { _size = Math.Max(value, 1D); }
        }

        public double ValueEquatesToSize
        {
            get { return _valueEquatesToSize; }
            set { _valueEquatesToSize = Math.Abs(value); }
        }

        public RenderChartType ChartType
        {
            get { return _type; }
            set { _type = value; }
        }

        public RenderSizeType SizeType
        {
            get { return _sizeType; }
            set { _sizeType = value; }
        }

        public SimpleLabelRenderer.RenderLabelPriority LabelPriority
        {
            get
            {
                return _labelPriority;
            }
            set
            {
                _labelPriority = value;
            }
        }

        #endregion

        #region ILabelRenderer Member

        public void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter)
        {
            if (layer.FeatureClass == null)
            {
                return;
            }

            foreach (string fieldName in _symbolTable.Keys)
            {
                if (layer.FeatureClass.Fields.FindField(fieldName) == null)
                {
                    continue;
                }

                filter.AddField(fieldName);
            }
            _clipEnvelope = new Envelope(display.DisplayTransformation.TransformedBounds(display));
            if (display.GeometricTransformer != null)
            {
                object e = display.GeometricTransformer.InvTransform2D(display.Envelope);
                if (e is IGeometry)
                {
                    _clipEnvelope = ((IGeometry)e).Envelope;
                }
            }
        }

        public bool CanRender(IFeatureLayer layer, IMap map)
        {
            if (layer.FeatureClass == null)
            {
                return false;
            }

            int count = 0;
            foreach (IField field in layer.FeatureClass.Fields.ToEnumerable())
            {
                if (field.type == FieldType.biginteger ||
                    field.type == FieldType.Double ||
                    field.type == FieldType.Float ||
                    field.type == FieldType.integer ||
                    field.type == FieldType.smallinteger)
                {
                    count++;
                }
            }
            return count > 0;
        }

        public string Name
        {
            get { return "Chart Renderer"; }
        }

        public LabelRenderMode RenderMode
        {
            get { return _labelPriority == SimpleLabelRenderer.RenderLabelPriority.Always ? LabelRenderMode.RenderWithFeature : LabelRenderMode.UseRenderPriority; }
        }

        public int RenderPriority
        {
            get { return 3 - (int)_labelPriority; }
        }

        public void Draw(IDisplay disp, IFeature feature)
        {
            #region Check Values
            if (_symbolTable.Count == 0)
            {
                return;
            }

            double[] values = new double[_symbolTable.Count];
            double valSum = 0.0, valMax = double.MinValue, valMin = double.MaxValue;
            #region Get Values
            int i = 0;
            foreach (string fieldname in _symbolTable.Keys)
            {
                object val = feature[fieldname];
                if (val == null || val == DBNull.Value)
                {
                    return;
                }

                try
                {
                    values[i++] = Convert.ToDouble(val);
                    valSum += Math.Abs(values[i - 1]);
                    valMax = Math.Max(valMax, values[i - 1]);
                    valMin = Math.Min(valMin, values[i - 1]);
                }
                catch
                {
                    return;
                }
            }
            #endregion
            #endregion

            IPoint point = null;
            if (feature.Shape is IPoint)
            {
                point = (IPoint)feature.Shape;
                if (disp.LabelEngine.TryAppend(disp,
                    ChartAnnotationPolygon(disp, point, values, valSum, valMin, valMax),
                    feature.Shape, _labelPriority != SimpleLabelRenderer.RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                {
                    DrawChart(disp, point, values, valSum, valMin, valMax);
                }
            }
            else if (feature.Shape is IPolyline)
            {
                IEnvelope dispEnv = _clipEnvelope;
                IPolyline pLine = (IPolyline)Clip.PerformClip(dispEnv, feature.Shape);

                if (pLine == null)
                {
                    return;
                }

                for (int iPath = 0; iPath < pLine.PathCount; iPath++)
                {
                    IPath path = pLine[iPath];
                    for (int iPoint = 0; iPoint < path.PointCount - 1; iPoint++)
                    {
                        IPoint p1 = path[iPoint];
                        IPoint p2 = path[iPoint + 1];
                        if (dispEnv.MinX <= p1.X && dispEnv.MaxX >= p1.X &&
                            dispEnv.MinY <= p1.Y && dispEnv.MaxY >= p1.Y &&
                            dispEnv.MinX <= p2.X && dispEnv.MaxX >= p2.X &&
                            dispEnv.MinY <= p2.Y && dispEnv.MaxY >= p2.Y)
                        {
                            point = new Point((p1.X + p2.X) * 0.5, (p1.Y + p2.Y) * 0.5);
                            if (disp.LabelEngine.TryAppend(disp,
                                ChartAnnotationPolygon(disp, point, values, valSum, valMin, valMax),
                                pLine, _labelPriority != SimpleLabelRenderer.RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                            {
                                DrawChart(disp, point, values, valSum, valMin, valMax);
                                return;
                            }
                        }
                    }
                }
            }
            else if (feature.Shape is IPolygon)
            {
                LabelPolygon(disp, (IPolygon)feature.Shape, values, valSum, valMin, valMax);
            }
        }

        #endregion

        private bool LabelPolygon(IDisplay disp, IPolygon polygon, double[] values, double valSum, double valMin, double valMax)
        {
            IEnvelope env = new Envelope(_clipEnvelope);

            if (polygon is ITopologicalOperation)
            {
                try
                {
                    double tolerance = 1.0 * disp.MapScale / disp.Dpm;  // 1 Pixel

                    // Wichtig bei Flächen mit sehr vielen Vertices... Bundesländer, Länder Thema kann sonst beim Clippen abstürzen
                    //if (polygon.TotalPointCount > MaxPolygonTotalPointCount)
                    //{
                    //polygon = SpatialAlgorithms.Algorithm.SnapOutsidePointsToEnvelope(polygon, env);
                    //polygon = (IPolygon)SpatialAlgorithms.Algorithm.Generalize(polygon, tolerance);
                    //}
                    // For testing polygons with many vertices an clipping
                    // polygon = (IPolygon)SpatialAlgorithms.Algorithm.InterpolatePoints(polygon, 2, true);

                    IGeometry g;
                    if (polygon.TotalPointCount > SimpleLabelRenderer.MaxPolygonTotalPointCount)
                    {
                        g = polygon;
                    }
                    else
                    {
                        ((ITopologicalOperation)polygon).Clip(env, out g);
                    }
                    if (g == null)
                    {
                        return false;
                    }

                    if (g is IPolygon)
                    {
                        polygon = (IPolygon)g;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            if (polygon == null)
            {
                return false;
            }

            IMultiPoint pColl = Algorithm.PolygonLabelPoints(polygon);
            return LabelPointCollection(disp, polygon, pColl, values, valSum, valMin, valMax);
        }

        private bool LabelPointCollection(IDisplay disp, IPolygon polygon, IMultiPoint pColl, double[] values, double valSum, double valMin, double valMax)
        {
            if (pColl == null)
            {
                return false;
            }

            for (int i = 0; i < pColl.PointCount; i++)
            {
                if (disp.LabelEngine.TryAppend(disp,
                    ChartAnnotationPolygon(disp, pColl[i], values, valSum, valMin, valMax),
                    pColl[i], _labelPriority != SimpleLabelRenderer.RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                {
                    DrawChart(disp, pColl[i], values, valSum, valMin, valMax);
                    return true;
                }
            }
            for (int i = 0; i < pColl.PointCount; i++)
            {
                ISmartLabelPoint slp = pColl[i] as ISmartLabelPoint;
                if (slp != null)
                {
                    LabelPointCollection(disp, polygon, slp.AlernativeLabelPoints(disp), values, valSum, valMin, valMax);
                }
            }
            return false;
        }

        private List<IAnnotationPolygonCollision> ChartAnnotationPolygon(IDisplay disp, IPoint point, double[] values, double valSum, double valMin, double valMax)
        {
            IEnvelope chartEnvelope = null;

            if (_type == RenderChartType.Pie)
            {
                #region Measure Pie

                if (valSum == 0.0)
                {
                    return null;
                }

                double r = disp.MapScale / disp.Dpm * _size / 2D;
                switch (_sizeType)
                {
                    case RenderSizeType.ValueOfEquatesToSize:
                        double a = r * r * Math.PI * valSum / _valueEquatesToSize;
                        r = Math.Sqrt(a / Math.PI);
                        break;
                }

                chartEnvelope = new Envelope(point.X - r, point.Y - r, point.X + r, point.Y + r);

                #endregion
            }
            else if (_type == RenderChartType.Bars)
            {
                #region Draw Bars

                double height = disp.MapScale / disp.Dpm * _size;
                double width = disp.MapScale / disp.Dpm * _size / 4D;
                double heightVal = _valueEquatesToSize;

                switch (_sizeType)
                {
                    case RenderSizeType.ConstantSize:
                        if (valSum == 0.0)
                        {
                            return null;
                        }

                        height *= _valueEquatesToSize / valSum;
                        break;
                }

                double left = -(_symbolTable.Count * width / 2.0) - (_symbolTable.Count - 1) * (disp.MapScale / disp.Dpm) / 2.0;

                chartEnvelope = new Envelope(point.X + left,
                    point.Y,
                    point.X + left + (width + disp.MapScale / disp.Dpm) * _symbolTable.Count,
                    point.Y + height * (valMax / heightVal));

                #endregion
            }
            else if (_type == RenderChartType.Stack)
            {
                #region Draw Stack

                double height = disp.MapScale / disp.Dpm * _size;
                double width = disp.MapScale / disp.Dpm * _size / 4D;
                double heightVal = _valueEquatesToSize;

                Path outerPath = new Path();

                switch (_sizeType)
                {
                    case RenderSizeType.ConstantSize:
                        if (valSum == 0.0)
                        {
                            return null;
                        }

                        height *= _valueEquatesToSize / valSum;
                        break;
                }

                double left = -width / 2.0;
                chartEnvelope = new Envelope(point.X + left,
                    point.Y,
                    point.X + left + width,
                    point.Y + height * (valSum / heightVal));

                #endregion
            }

            if (chartEnvelope != null)
            {
                if (disp.GeometricTransformer != null)
                {
                    object e = disp.GeometricTransformer.Transform2D(chartEnvelope);
                    if (e is IGeometry)
                    {
                        chartEnvelope = ((IGeometry)e).Envelope;
                    }
                }

                double x1 = chartEnvelope.MinX, y1 = chartEnvelope.MaxY,
                       x2 = chartEnvelope.MaxX, y2 = chartEnvelope.MinY;
                disp.World2Image(ref x1, ref y1);
                disp.World2Image(ref x2, ref y2);

                return new List<IAnnotationPolygonCollision>()
                {
                    new AnnotationPolygon((float)x1,(float)y1,(float)Math.Abs(x2-x1),(float)Math.Abs(y2-y1))
                };
            }
            return null;
        }

        private void DrawChart(IDisplay disp, IPoint point, double[] values, double valSum, double valMin, double valMax)
        {
            if (!(disp is Display) || disp.LabelEngine.LabelCanvas == null)
            {
                return;
            }

            var originalCanvas = disp.Canvas;
            try
            {
                ((Display)disp).Canvas = disp.LabelEngine.LabelCanvas;

                if (_symbolTable.Count == 0)
                {
                    return;
                }

                if(disp.GeometricTransformer != null)
                {
                    point = disp.GeometricTransformer.Transform2D(point) as IPoint;
                    if(point is null)
                    {
                        return;
                    }
                }

                int i = 0;
                if (_type == RenderChartType.Pie)
                {
                    #region Draw Pie

                    if (valSum == 0.0)
                    {
                        return;
                    }

                    double r = disp.MapScale / disp.Dpm * _size / 2D;
                    switch (_sizeType)
                    {
                        case RenderSizeType.ValueOfEquatesToSize:
                            double a = r * r * Math.PI * valSum / _valueEquatesToSize;
                            r = Math.Sqrt(a / Math.PI);
                            break;
                    }

                    i = 0;
                    double startAngle = 0.0;

                    Path outerPath = new Path();
                    foreach (string fieldname in _symbolTable.Keys)
                    {
                        ISymbol symbol = _symbolTable[fieldname];

                        double angle = Math.PI * 2.0 * Math.Abs(values[i++]) / valSum;

                        Polygon poly = new Polygon();
                        Ring ring = new Ring();
                        poly.AddRing(ring);
                        ring.AddPoint(point);

                        for (double a = startAngle, to = startAngle + angle; a < to; a += 0.01745)
                        {
                            IPoint piePoint = new Point(point.X + r * Math.Cos(a), point.Y + r * Math.Sin(a));
                            ring.AddPoint(piePoint);
                            outerPath.AddPoint(piePoint);
                        }
                        ring.AddPoint(new Point(point.X + r * Math.Cos(startAngle + angle), point.Y + r * Math.Sin(startAngle + angle)));

                        symbol.Draw(disp, poly);

                        //if (_outlineSymbol != null && ring.PointCount > 0)
                        //    outerPath.AddPoints(ring);

                        startAngle += angle;
                    }
                    if (outerPath.PointCount > 0 && _outlineSymbol != null)
                    {
                        outerPath.AddPoint(outerPath[0]);
                        _outlineSymbol.Draw(disp, new Polyline(outerPath));
                    }

                    #endregion
                }
                else if (_type == RenderChartType.Bars)
                {
                    #region Draw Bars

                    double height = disp.MapScale / disp.Dpm * _size;
                    double width = disp.MapScale / disp.Dpm * _size / 4D;
                    double heightVal = _valueEquatesToSize;

                    switch (_sizeType)
                    {
                        case RenderSizeType.ConstantSize:
                            if (valSum == 0.0)
                            {
                                return;
                            }

                            height *= _valueEquatesToSize / valSum;
                            break;
                    }

                    i = 0;
                    double left = -(_symbolTable.Count * width / 2.0) - (_symbolTable.Count - 1) * (disp.MapScale / disp.Dpm) / 2.0;
                    foreach (string fieldname in _symbolTable.Keys)
                    {
                        ISymbol symbol = _symbolTable[fieldname];

                        double h = height * (values[i++] / heightVal);
                        if (Math.Abs(h) < disp.MapScale / disp.Dpm)
                        {
                            h = disp.MapScale / disp.Dpm;
                        }

                        Polygon poly = new Polygon();
                        Ring ring = new Ring();
                        poly.AddRing(ring);
                        ring.AddPoint(new Point(point.X + left, point.Y));
                        ring.AddPoint(new Point(point.X + left, point.Y + h));
                        ring.AddPoint(new Point(point.X + left + width, point.Y + h));
                        ring.AddPoint(new Point(point.X + left + width, point.Y));

                        symbol.Draw(disp, poly);

                        if (_outlineSymbol != null && ring.PointCount > 0)
                        {
                            ring.AddPoint(new Point(point.X + left, point.Y));
                            _outlineSymbol.Draw(disp, new Polyline(ring));
                        }
                        left += width + disp.MapScale / disp.Dpm;
                    }

                    #endregion
                }
                else if (_type == RenderChartType.Stack)
                {
                    #region Draw Stack
                    double height = disp.MapScale / disp.Dpm * _size;
                    double width = disp.MapScale / disp.Dpm * _size / 4D;
                    double heightVal = _valueEquatesToSize;

                    Path outerPath = new Path();

                    switch (_sizeType)
                    {
                        case RenderSizeType.ConstantSize:
                            if (valSum == 0.0)
                            {
                                return;
                            }

                            height *= _valueEquatesToSize / valSum;
                            break;
                    }

                    i = 0;
                    double left = -width / 2.0, bottom = 0D;
                    foreach (string fieldname in _symbolTable.Keys)
                    {
                        ISymbol symbol = _symbolTable[fieldname];

                        double h = height * (values[i++] / heightVal);
                        if (Math.Abs(h) < disp.MapScale / disp.Dpm)
                        {
                            h = disp.MapScale / disp.Dpm;
                        }

                        Polygon poly = new Polygon();
                        Ring ring = new Ring();
                        poly.AddRing(ring);
                        ring.AddPoint(new Point(point.X + left, point.Y + bottom));
                        ring.AddPoint(new Point(point.X + left, point.Y + bottom + h));
                        ring.AddPoint(new Point(point.X + left + width, point.Y + bottom + h));
                        ring.AddPoint(new Point(point.X + left + width, point.Y + bottom));

                        if (_outlineSymbol != null)
                        {
                            if (i == 1)
                            {
                                outerPath.AddPoint(new Point(point.X + left, point.Y + bottom));
                            }
                            if (i == _symbolTable.Count)
                            {
                                outerPath.AddPoint(new Point(point.X + left, point.Y + bottom + h));
                                outerPath.AddPoint(new Point(point.X + left + width, point.Y + bottom + h));
                                outerPath.AddPoint(new Point(point.X + left + width, point.Y));
                                outerPath.AddPoint(outerPath[0]);
                            }
                        }

                        symbol.Draw(disp, poly);
                        bottom += h;
                    }
                    if (_outlineSymbol != null && outerPath.PointCount > 0)
                    {
                        _outlineSymbol.Draw(disp, new Polyline(outerPath));
                    }
                    #endregion
                }
            }
            finally
            {
                ((Display)disp).Canvas = originalCanvas;
            }
        }

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get { return new List<ISymbol>(); }
        }

        public bool Combine(IRenderer renderer)
        {
            return false;
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _type = (RenderChartType)stream.Load("Type", (int)RenderChartType.Pie);
            _sizeType = (RenderSizeType)stream.Load("SizeType", (int)RenderSizeType.ConstantSize);
            _labelPriority = (SimpleLabelRenderer.RenderLabelPriority)stream.Load("labelPriority", (int)SimpleLabelRenderer.RenderLabelPriority.Normal);
            _size = (double)stream.Load("Size", 50D);
            _valueEquatesToSize = (double)stream.Load("ValueEquatesToSize", 100D);

            ValueMapRendererSymbol sym;
            while ((sym = (ValueMapRendererSymbol)stream.Load("ChartSymbol", null, new ValueMapRendererSymbol())) != null)
            {
                SetSymbol(sym._key, sym._symbol);
            }
            _outlineSymbol = stream.Load("Outline", null) as ILineSymbol;
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Type", (int)_type);
            stream.Save("SizeType", (int)_sizeType);
            stream.Save("labelPriority", (int)_labelPriority);
            stream.Save("Size", _size);
            stream.Save("ValueEquatesToSize", _valueEquatesToSize);

            foreach (string key in _symbolTable.Keys)
            {
                ValueMapRendererSymbol sym = new ValueMapRendererSymbol(key, _symbolTable[key]);
                stream.Save("ChartSymbol", sym);
            }
            if (_outlineSymbol != null)
            {
                stream.Save("Outline", _outlineSymbol);
            }
        }

        #endregion

        #region IClone2 Member

        public object Clone(CloneOptions options)
        {
            if(options is not null)
            {
                options = new CloneOptions(options.Display,
                    options.ApplyRefScale,
                    options.MaxLabelRefScaleFactor,  // use maxLabelRefscaleFactor also for symbols
                    options.MaxLabelRefScaleFactor); // otherwise they will use the FeatureRefscalefactor
                                                     // wich is wrong here
            }

            ChartLabelRenderer clone = new ChartLabelRenderer();

            foreach (string key in _symbolTable.Keys)
            {
                ISymbol symbol = _symbolTable[key];
                if (symbol != null)
                {
                    symbol = (ISymbol)symbol.Clone(options);
                }

                clone._symbolTable.Add(key, symbol);
            }
            clone._labelPriority = _labelPriority;
            clone._type = _type;
            clone._outlineSymbol = _outlineSymbol != null ? (ILineSymbol)_outlineSymbol.Clone(options) : null;

            float fac = 1f;
            var display = options?.Display;

            if (options.ApplyRefScale)
            {
                fac = ReferenceScaleHelper.RefscaleFactor(
                    (float)(display.ReferenceScale / display.MapScale),
                    (float)Size,
                    0f,
                    0f);
                fac = options.LabelRefScaleFactor(fac);
            }

            clone._size = Math.Max(_size * fac, 1D);
            clone._valueEquatesToSize = _valueEquatesToSize;
            clone._sizeType = _sizeType;
            clone._clipEnvelope = _clipEnvelope != null ? new Envelope(_clipEnvelope) : null;
            return clone;
        }

        public void Release()
        {
            foreach (string key in _symbolTable.Keys)
            {
                ISymbol symbol = _symbolTable[key];
                if (symbol == null)
                {
                    continue;
                }

                symbol.Release();
            }
            _symbolTable.Clear();
        }

        #endregion

        #region IPriority Member

        public int Priority
        {
            get
            {
                switch (_labelPriority)
                {
                    case SimpleLabelRenderer.RenderLabelPriority.Always:
                        return 0;
                    case SimpleLabelRenderer.RenderLabelPriority.High:
                        return 100;
                    case SimpleLabelRenderer.RenderLabelPriority.Normal:
                        return 0;
                    case SimpleLabelRenderer.RenderLabelPriority.Low:
                        return -100;
                }
                return 0;
            }
        }

        #endregion

        #region ICreateDefault Members

        public ValueTask DefaultIfEmpty(object initObject)
        {
            return ValueTask.CompletedTask;
        }

        #endregion

        #region ILegendGroup Member

        public int LegendItemCount
        {
            get { return _symbolTable.Count; }
        }

        public ILegendItem LegendItem(int index)
        {
            if (index < 0)
            {
                return null;
            }

            if (index <= _symbolTable.Count)
            {
                int count = 0;
                foreach (object lItem in _symbolTable.Values)
                {
                    if (count == index && lItem is ILegendItem)
                    {
                        return (LegendItem)lItem;
                    }

                    count++;
                }
            }
            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (item == symbol)
            {
                return;
            }

            foreach (string key in _symbolTable.Keys)
            {
                if (!(_symbolTable[key] is ILegendItem))
                {
                    continue;
                }

                if (_symbolTable[key] == item)
                {
                    if (symbol is ILegendItem)
                    {
                        symbol.LegendLabel = item.LegendLabel;
                    }

                    _symbolTable[key] = symbol;
                    return;
                }
            }
        }

        #endregion
    }
}
