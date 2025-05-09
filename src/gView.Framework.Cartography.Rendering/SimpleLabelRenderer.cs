﻿using gView.Framework.Cartography.Rendering.Exntensions;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.UI;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using gView.Framework.Symbology;
using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.Rendering
{
    [RegisterPlugIn("92650F7D-CEC9-4418-9EA0-A8B09436AA7A")]
    public class SimpleLabelRenderer : Cloner, ILabelRenderer, ILegendGroup, IDefault, IPriority
    {
        public enum RenderHowManyLabels 
        {
            OnPerName = 0,
            OnPerFeature = 1,
            OnPerPart = 2 
        }
        public enum RenderLabelPriority 
        {
            [Description("Always (no overlay check)")]
            Always = 0,
            [Description("High (overlay check)")]
            High = 1,
            [Description("Normal (overlay check)")]
            Normal = 2,
            [Description("Low (overlay check)")]
            Low = 3 
        
        }
        public enum CartographicLineLabeling 
        { 
            Parallel = 0, 
            CurvedText = 1, 
            Horizontal = 2, 
            Perpendicular = 3 
        }
        public enum CartographicPolygonLabelling 
        { 
            Horizontal = 0, 
            Parallel = 1 
        }

        private string _fieldname, _sizeField, _fontField, _expression;
        private bool _useExpression = false; 
        private ITextSymbol _symbol;

        private RenderHowManyLabels _howManyLabels = RenderHowManyLabels.OnPerFeature;
        private RenderLabelPriority _labelPriority = RenderLabelPriority.Normal;
        private List<string> _labelStrings = new List<string>();
        private SymbolRotation _symbolRotation;
        private CartographicLineLabeling _lineLabelling = CartographicLineLabeling.Parallel;

        public SimpleLabelRenderer()
        {
            _fieldname = _sizeField = _fontField = string.Empty;
            _symbol = new SimpleTextSymbol();

            _symbolRotation = new SymbolRotation();
        }

        protected SimpleLabelRenderer(ITextSymbol symbol, string fieldname)
        {
            _symbol = symbol;
            _fieldname = fieldname;
            _sizeField = _fontField = string.Empty;
        }

        public string FieldName
        {
            get { return _fieldname; }
            set { _fieldname = value; }
        }

        public string SizeFieldName
        {
            get { return _sizeField; }
            set { _sizeField = value; }
        }

        public string FontField
        {
            get { return _fontField; }
            set { _fontField = value; }
        }

        public string LabelExpression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        public bool UseExpression
        {
            get { return _useExpression; }
            set { _useExpression = value; }
        }

        public RenderHowManyLabels HowManyLabels
        {
            get
            {
                return _howManyLabels;
            }
            set
            {
                _howManyLabels = value;
            }
        }

        public RenderLabelPriority LabelPriority
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

        public CartographicLineLabeling CartoLineLabelling
        {
            get { return _lineLabelling; }
            set { _lineLabelling = value; }
        }

        public ITextSymbol TextSymbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        public SymbolRotation SymbolRotation
        {
            get { return _symbolRotation; }
            set
            {
                if (value == null)
                {
                    _symbolRotation.RotationFieldName = "";
                }
                else
                {
                    _symbolRotation = value;
                }
            }
        }

        
        private IEnvelope _clipEnvelope = null;

        virtual protected bool BeforeRenderFeature(IDisplay display, IFeature feature) => true;
        virtual protected string ModifyEvaluatedLabel(IDisplay display, IFeature feature, string label) => label;

        #region ILabelRenderer Members

        virtual public void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter)
        {
            if (layer.FeatureClass == null)
            {
                return;
            }

            if (_useExpression)
            {
                foreach (string fieldname in _expression.ExtractFieldNames())
                {
                    if (layer.FeatureClass.FindField(fieldname) != null)
                    {
                        filter.AddField(fieldname);
                    }
                }
            }
            else
            {
                if (layer.FeatureClass.FindField(_fieldname) != null)
                {
                    filter.AddField(_fieldname);
                }
            }
            if (_sizeField != string.Empty)
            {
                filter.AddField(_sizeField);
            }

            if (_fontField != string.Empty)
            {
                filter.AddField(_fontField);
            }

            if (layer.FeatureClass.FindField(_symbolRotation.RotationFieldName) != null)
            {
                filter.AddField(_symbolRotation.RotationFieldName);
            }

            _clipEnvelope = new Envelope(display.DisplayTransformation.RotatedBounds());
            if (display.GeometricTransformer != null)
            {
                object e = display.GeometricTransformer.InvTransform2D(_clipEnvelope);
                if (e is IGeometry)
                {
                    _clipEnvelope = ((IGeometry)e).Envelope;
                }
            }
        }

        public bool CanRender(IFeatureLayer layer, IMap map)
        {
            return true;
        }

        public string Name
        {
            get { return "Simple Text Renderer"; }
        }

        public LabelRenderMode RenderMode
        {
            get
            {
                return _labelPriority == RenderLabelPriority.Always ? LabelRenderMode.RenderWithFeature : LabelRenderMode.UseRenderPriority;
            }
        }

        public int RenderPriority
        {
            get
            {
                return 3 - (int)_labelPriority;
            }
        }

        public void Draw(IDisplay display, IFeatureLayer layer, IFeature feature)
        {
            if (!BeforeRenderFeature(display, feature) || !(_symbol is ISymbol))
            {
                return;
            }

            string expr = _expression;
            foreach (FieldValue fv in feature.Fields)
            {
                if (fv.Name == _fieldname && !_useExpression)
                {
                    _symbol.Text = fv.Value.ToString();
                }
                if (fv.Name == _symbolRotation.RotationFieldName)
                {
                    try
                    {
                        _symbol.Rotation = (float)_symbolRotation.Convert2DEGAritmetic(Convert.ToDouble(fv.Value));
                    }
                    catch { }
                }
                if (_useExpression)
                {
                    expr = expr.EvaluateExpression(fv);
                }
            }
            if (_useExpression)
            {
                if (SimpleScriptInterpreter.IsSimpleScript(expr))
                {
                    expr = new SimpleScriptInterpreter(expr).Interpret();
                }

                _symbol.Text = ModifyEvaluatedLabel(display, feature, expr);
            }

            if (string.IsNullOrWhiteSpace(_symbol.Text))
            {
                return;
            }

            if (_howManyLabels == RenderHowManyLabels.OnPerName)
            {
                if (_labelStrings.Contains(_symbol.Text.Trim()))
                {
                    return;
                }
            }

            if (feature.Shape is IPoint)
            {
                if (display.LabelEngine.TryAppend(display, layer, _symbol, feature.Shape, _labelPriority != RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                {
                    if (_howManyLabels == RenderHowManyLabels.OnPerName)
                    {
                        _labelStrings.Add(_symbol.Text.Trim());
                    }
                }
            }
            else if (feature.Shape is IMultiPoint)
            {
                var multiPoint = (IMultiPoint)feature.Shape;
                for (int i = 0, i_to = multiPoint.PointCount; i < i_to; i++)
                {
                    if (multiPoint[i] == null)
                    {
                        continue;
                    }

                    if (display.LabelEngine.TryAppend(display, layer, _symbol, multiPoint[i], _labelPriority != RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                    {
                        if (_howManyLabels == RenderHowManyLabels.OnPerName)
                        {
                            _labelStrings.Add(_symbol.Text.Trim());
                        }

                        if (_howManyLabels == RenderHowManyLabels.OnPerPart)
                        {
                            break;
                        }
                    }
                }
            }
            else if (feature.Shape is IPolyline)
            {
                IPoint point1 = null, point2 = null;
                double maxLenght2 = 0;

                IEnvelope dispEnv = _clipEnvelope; //disp.Envelope;
                //if (disp.GeometricTransformer != null)
                //{
                //    object e = disp.GeometricTransformer.InvTransform2D(disp.Envelope);
                //    if (e is IGeometry) dispEnv = ((IGeometry)e).Envelope;
                //}
                IPolyline pLine = (IPolyline)Clip.PerformClip(dispEnv, feature.Shape);

                if (pLine == null)
                {
                    return;
                }

                var lineLabelling = (_lineLabelling, _symbol.Text.Length) switch
                {
                    // dont curve short names
                    (CartographicLineLabeling.CurvedText, <= 5) => CartographicLineLabeling.Parallel,
                    _ => _lineLabelling
                };

                if (lineLabelling == CartographicLineLabeling.CurvedText)
                {
                    #region Text On Path

                    IDisplayCharacterRanges ranges = _symbol.MeasureCharacterWidth(display);
                    if (ranges == null)
                    {
                        return;
                    }

                    float textWidth = ranges.Width;

                    for (int iPath = 0; iPath < pLine.PathCount; iPath++)
                    {
                        IPath path = pLine[iPath];
                        if (path == null)
                        {
                            continue;
                        }

                        IPointCollection pathPoints = path;
                        if (display.GeometricTransformer != null)
                        {
                            pathPoints = (IPointCollection)display.GeometricTransformer.Transform2D(pathPoints);
                        }

                        DisplayPath displayPath = new DisplayPath();
                        for (int iPoint = 0; iPoint < pathPoints.PointCount; iPoint++)
                        {
                            double x = pathPoints[iPoint].X, y = pathPoints[iPoint].Y;
                            display.World2Image(ref x, ref y);
                            displayPath.AddPoint(new GraphicsEngine.CanvasPointF((float)x, (float)y));
                        }
                        float pathLenght = displayPath.Length;
                        if (pathLenght == 0.0 || textWidth > pathLenght)
                        {
                            continue;
                        }

                        displayPath.Chainage = pathLenght / 2f - textWidth / 2f;
                        float nextChainage = pathLenght / 10f;
                        bool found = false;
                        while (!found)
                        {
                            if (display.LabelEngine.TryAppend(display, layer, _symbol, displayPath, _labelPriority != RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                            {
                                found = true;
                                if (_howManyLabels == RenderHowManyLabels.OnPerName)
                                {
                                    _labelStrings.Add(_symbol.Text.Trim());
                                }

                                if (_howManyLabels != RenderHowManyLabels.OnPerPart)
                                {
                                    break;
                                }
                            }
                            if (!found)
                            {
                                displayPath.Chainage = nextChainage;
                                nextChainage += pathLenght / 10f;
                            }
                            if (displayPath.Chainage + textWidth > pathLenght)
                            {
                                break;
                            }
                        }
                    }
                    #endregion
                }
                else if (lineLabelling == CartographicLineLabeling.Horizontal ||
                         lineLabelling == CartographicLineLabeling.Perpendicular)
                {
                    #region Horizontal

                    bool found = false;

                    for (int iPath = 0; iPath < pLine.PathCount; iPath++)
                    {
                        IPath path = pLine[iPath];
                        if (path.PointCount < 2)
                        {
                            continue;
                        }

                        switch (TextSymbol.TextSymbolAlignment)
                        {
                            case TextSymbolAlignment.rightAlignOver:
                            case TextSymbolAlignment.rightAlignCenter:
                            case TextSymbolAlignment.rightAlignUnder:
                                point1 = path[1];
                                point2 = path[0];  // this will be alignment point in TextSymbol
                                found = true;
                                break;
                            case TextSymbolAlignment.leftAlignOver:
                            case TextSymbolAlignment.leftAlignCenter:
                            case TextSymbolAlignment.leftAlignUnder:
                                point1 = path[path.PointCount - 1];  // this will be alignment point in Textsymbol
                                point2 = path[path.PointCount - 2];
                                found = true;
                                break;
                            default:
                                for (int iPoint = 0, iPointTo = path.PointCount - 1; iPoint < iPointTo; iPoint++)
                                {
                                    IPoint p1 = path[iPoint];
                                    IPoint p2 = path[iPoint + 1];
                                    if (dispEnv.MinX <= p1.X && dispEnv.MaxX >= p1.X &&
                                        dispEnv.MinY <= p1.Y && dispEnv.MaxY >= p1.Y &&
                                        dispEnv.MinX <= p2.X && dispEnv.MaxX >= p2.X &&
                                        dispEnv.MinY <= p2.Y && dispEnv.MaxY >= p2.Y)
                                    {
                                        double len = /*Math.Sqrt*/(p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
                                        if (len > maxLenght2)
                                        {
                                            maxLenght2 = len;
                                            point1 = p1;
                                            point2 = p2;
                                        }
                                    }
                                }
                                break;
                        }
                        if (found == true)
                        {
                            break;
                        }
                    }
                    if (point1 != null && point2 != null)
                    {
                        Point p = new Point(point1.X * 0.5 + point2.X * 0.5,
                                            point1.Y * 0.5 + point2.Y * 0.5);

                        if (lineLabelling == CartographicLineLabeling.Perpendicular)
                        {
                            double angle = Math.Atan2(point2.X - point1.X, point2.Y - point1.Y) * 180.0 / Math.PI;
                            if (angle < 0)
                            {
                                angle += 360;
                            }

                            //if (angle > 90 && angle < 270)
                            //{
                            //    angle -= 180;
                            //}

                            _symbol.Angle = (float)angle;
                        }
                        if (display.LabelEngine.TryAppend(display, layer, _symbol, p, _labelPriority != RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                        {
                            if (_howManyLabels == RenderHowManyLabels.OnPerName)
                            {
                                _labelStrings.Add(_symbol.Text.Trim());
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    #region Parallel Labelling

                    bool found = false;
                    for (int iPath = 0; iPath < pLine.PathCount; iPath++)
                    {
                        IPath path = pLine[iPath];
                        if (path.PointCount < 2)
                        {
                            continue;
                        }

                        switch (TextSymbol.TextSymbolAlignment)
                        {
                            case TextSymbolAlignment.rightAlignOver:
                            case TextSymbolAlignment.rightAlignCenter:
                            case TextSymbolAlignment.rightAlignUnder:
                                point1 = path[1];
                                point2 = path[0];  // this will be alignment point in TextSymbol
                                found = true;
                                break;
                            case TextSymbolAlignment.leftAlignOver:
                            case TextSymbolAlignment.leftAlignCenter:
                            case TextSymbolAlignment.leftAlignUnder:
                                point1 = path[path.PointCount - 1];  // this will be alignment point in Textsymbol
                                point2 = path[path.PointCount - 2];
                                found = true;
                                break;
                            default:
                                for (int iPoint = 0, iPointTo = path.PointCount - 1; iPoint < iPointTo; iPoint++)
                                {
                                    IPoint p1 = path[iPoint];
                                    IPoint p2 = path[iPoint + 1];
                                    if (dispEnv.MinX <= p1.X && dispEnv.MaxX >= p1.X &&
                                        dispEnv.MinY <= p1.Y && dispEnv.MaxY >= p1.Y &&
                                        dispEnv.MinX <= p2.X && dispEnv.MaxX >= p2.X &&
                                        dispEnv.MinY <= p2.Y && dispEnv.MaxY >= p2.Y)
                                    {
                                        double len2 = /*Math.Sqrt*/(p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
                                        if (len2 > maxLenght2)
                                        {
                                            maxLenght2 = len2;
                                            //alpha = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X));
                                            point1 = p1;
                                            point2 = p2;
                                        }
                                    }
                                }
                                break;
                        }

                        if (found)
                        {
                            break;
                        }
                    }
                    if (point1 != null && point2 != null)
                    {
                        pLine = new Polyline();
                        pLine.AddPath(new Path());
                        pLine[0].AddPoint(point1);
                        pLine[0].AddPoint(point2);

                        if (display.LabelEngine.TryAppend(display, layer, _symbol, pLine, _labelPriority != RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                        {
                            if (_howManyLabels == RenderHowManyLabels.OnPerName)
                            {
                                _labelStrings.Add(_symbol.Text.Trim());
                            }
                        }
                    }

                    #endregion
                }
            }
            else if (feature.Shape is IPolygon)
            {
                LabelPolygon(display, layer, (IPolygon)feature.Shape);
            }
        }

        #endregion

        internal const int MaxPolygonTotalPointCount = 12000;  // Labelling is too expensive for complex polygons
        private bool LabelPolygon(IDisplay disp, IFeatureLayer layer, IPolygon polygon)
        {
            //var center = new MultiPoint();
            //center.AddPoint(polygon.Envelope.Center);
            //return LabelPointCollection(disp, polygon, center);

            IEnvelope env = new Envelope(_clipEnvelope); //new Envelope(disp.Envelope);
            //env.Raise(70.0);
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
                    if (polygon.TotalPointCount > MaxPolygonTotalPointCount)
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

            if (polygon.TotalPointCount < MaxPolygonTotalPointCount)
            {
                IMultiPoint pColl = Algorithm.PolygonLabelPoints(polygon);
                return LabelPointCollection(disp, layer, polygon, pColl);
            }
            else
            {
                return false;
            }
        }

        private bool LabelPointCollection(IDisplay disp, IFeatureLayer layer, IPolygon polygon, IMultiPoint pColl)
        {
            if (pColl == null)
            {
                return false;
            }

            switch (_howManyLabels)
            {
                case RenderHowManyLabels.OnPerFeature:
                case RenderHowManyLabels.OnPerName:
                    for (int i = 0; i < pColl.PointCount; i++)
                    {
                        if (disp.LabelEngine.TryAppend(disp, layer, _symbol, pColl[i], _labelPriority != RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                        {
                            if (_howManyLabels == RenderHowManyLabels.OnPerName)
                            {
                                _labelStrings.Add(_symbol.Text.Trim());
                            }

                            return true;
                        }
                    }
                    for (int i = 0; i < pColl.PointCount; i++)
                    {
                        ISmartLabelPoint slp = pColl[i] as ISmartLabelPoint;
                        if (slp != null)
                        {
                            LabelPointCollection(disp, layer, polygon, slp.AlernativeLabelPoints(disp));
                        }
                    }
                    break;
                case RenderHowManyLabels.OnPerPart:
                    if (disp.LabelEngine.TryAppend(disp, layer, _symbol, pColl, _labelPriority != RenderLabelPriority.Always) == LabelAppendResult.Succeeded)
                    {
                        if (_howManyLabels == RenderHowManyLabels.OnPerName)
                        {
                            _labelStrings.Add(_symbol.Text.Trim());
                        }

                        return true;
                    }
                    break;
            }

            return false;
        }

        #region IPersistable Members

        public void Load(IPersistStream stream)
        {
            _fieldname = (string)stream.Load("Fieldname");
            _sizeField = (string)stream.Load("Sizefield");
            _fontField = (string)stream.Load("Fontfield");
            _expression = (string)stream.Load("Expression", "");
            _useExpression = (bool)stream.Load("UseExpression", false);

            _symbol = (ITextSymbol)stream.Load("Symbol");

            _howManyLabels = (RenderHowManyLabels)stream.Load("howManyLabels", (int)RenderHowManyLabels.OnPerFeature);
            _labelPriority = (RenderLabelPriority)stream.Load("labelPriority", (int)RenderLabelPriority.Normal);
            _lineLabelling = (CartographicLineLabeling)stream.Load("lineLabelling", (int)CartographicLineLabeling.Parallel);

            _symbolRotation = (SymbolRotation)stream.Load("SymbolRotation", _symbolRotation, _symbolRotation);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Fieldname", _fieldname);
            stream.Save("Sizefield", _sizeField);
            stream.Save("Fontfield", _fontField);
            stream.Save("Expression", _expression);
            stream.Save("UseExpression", _useExpression);

            stream.Save("Symbol", _symbol);

            stream.Save("howManyLabels", (int)_howManyLabels);
            stream.Save("labelPriority", (int)_labelPriority);
            stream.Save("lineLabelling", (int)_lineLabelling);

            if (_symbolRotation.RotationFieldName != "")
            {
                stream.Save("SymbolRotation", _symbolRotation);
            }
        }

        #endregion

        #region IClone2 Members

        virtual protected SimpleLabelRenderer CreateCloneInstance(CloneOptions options)
            => new SimpleLabelRenderer(
                (ITextSymbol)(_symbol is IClone2 ? _symbol.Clone(options) : null),
                _fieldname);

        public object Clone(CloneOptions options)
        {
            SimpleLabelRenderer renderer = CreateCloneInstance(options);

            renderer._howManyLabels = _howManyLabels;
            renderer._labelPriority = _labelPriority;
            renderer._fontField = _fontField;
            renderer._sizeField = _sizeField;
            renderer._expression = _expression;
            renderer._useExpression = _useExpression;
            renderer._symbolRotation = (SymbolRotation)_symbolRotation.Clone();
            renderer._clipEnvelope = _clipEnvelope != null ? new Envelope(_clipEnvelope) : null;
            renderer._lineLabelling = _lineLabelling;

            return renderer;
        }

        public void Release()
        {
            if (_symbol is ISymbol)
            {
                _symbol.Release();
            }

            _symbol = null;
            _labelStrings.Clear();
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
            get { return _symbol != null ? 1 : 0; }
        }

        public ILegendItem LegendItem(int index)
        {
            if (index == 0)
            {
                return _symbol;
            }

            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (item == _symbol && symbol is ITextSymbol)
            {
                _symbol = symbol as ITextSymbol;
            }
        }

        #endregion

        #region IPriority Member

        public int Priority
        {
            get
            {
                switch (_labelPriority)
                {
                    case RenderLabelPriority.Always:
                        return 0;
                    case RenderLabelPriority.High:
                        return 100;
                    case RenderLabelPriority.Normal:
                        return 0;
                    case RenderLabelPriority.Low:
                        return -100;
                }
                return 0;
            }
        }

        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get { return new List<ISymbol>(new ISymbol[] { _symbol }); }
        }

        public bool Combine(IRenderer renderer)
        {
            return false;
        }
        #endregion
    }
}
