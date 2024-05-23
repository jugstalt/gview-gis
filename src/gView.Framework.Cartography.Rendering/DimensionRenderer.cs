using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.Common;
using gView.Framework.Core.UI;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.Rendering
{
    [RegisterPlugIn("C1880775-C5B7-40DD-80B3-6B54A1F41655")]
    public class DimensionRenderer : Cloner, IFeatureRenderer, IDefault, ILegendGroup
    {
        public enum DimensionLineCapType { Arrow = 0, ArrowLine = 1, Line = 2, Circle = 3 }

        private ITextSymbol _textSymbol = null;
        private ILineSymbol _lineSymbol = null;
        private bool _useRefScale = true;
        private DimensionLineCapType _capType = DimensionLineCapType.ArrowLine;
        private string _format = "0.00";

        public DimensionRenderer()
        {
            _textSymbol = new SimpleTextSymbol();
            _lineSymbol = new SimpleLineSymbol();
            ((SimpleLineSymbol)_lineSymbol).Color = GraphicsEngine.ArgbColor.Black;
        }

        internal DimensionRenderer(IDisplay display, DimensionRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            _useRefScale = renderer._useRefScale;
            _capType = renderer._capType;

            if (renderer != null && renderer._textSymbol != null)
            {
                _textSymbol = renderer._textSymbol.Clone(_useRefScale ? new CloneOptions(display, true) : null) as ITextSymbol;
            }
            if (renderer != null && renderer._lineSymbol != null)
            {
                _lineSymbol = renderer._lineSymbol.Clone(_useRefScale ? new CloneOptions(display, true) : null) as ILineSymbol;
            }

            _format = renderer._format;
        }

        public ITextSymbol TextSymbol
        {
            get { return _textSymbol; }
            set
            {
                if (_textSymbol != value)
                {
                    if (_textSymbol != null)
                    {
                        _textSymbol.Release();
                    }

                    _textSymbol = value;
                }
            }
        }

        public ILineSymbol LineSymbol
        {
            get { return _lineSymbol; }
            set
            {
                if (_lineSymbol != value)
                {
                    if (_lineSymbol != null)
                    {
                        _lineSymbol.Release();
                    }

                    _lineSymbol = value;
                }
            }
        }

        public DimensionLineCapType LineCapType
        {
            get { return _capType; }
            set { _capType = value; }
        }

        public string LabelFormat
        {
            get { return _format; }
            set { _format = value; }
        }

        #region IFeatureRenderer Member

        public void Draw(IDisplay disp, IFeature feature)
        {
            if (disp == null || disp.MapScale < 1.0 ||
                feature == null || !(feature.Shape is IPolyline))
            {
                return;
            }

            IPolyline fpLine = feature.Shape as IPolyline;

            Polyline pLine = new Polyline();
            Path path = new Path();
            path.AddPoint(new Point());
            path.AddPoint(new Point());

            pLine.AddPath(path);
            double l = 7.0 * disp.MapScale / disp.Dpm;  // [m]
            if (disp.ReferenceScale >= 1.0)
            {
                l *= disp.ReferenceScale / disp.MapScale;
            }

            for (int i = 0; i < fpLine.PathCount; i++)
            {
                IPath fPath = fpLine[i];
                if (fPath == null || fPath.PointCount < 2)
                {
                    continue;
                }

                path[0].X = fPath[0].X;
                path[0].Y = fPath[0].Y;
                double al1 = 0.0, al2 = 0.0;
                for (int p = 1; p < fPath.PointCount; p++)
                {
                    path[1].X = fPath[p].X;
                    path[1].Y = fPath[p].Y;

                    disp.Draw(_lineSymbol, pLine);

                    al1 = Math.Atan2(path[1].Y - path[0].Y, path[1].X - path[0].X);
                    if (p < fPath.PointCount - 1)
                    {
                        al2 = Math.Atan2(fPath[p + 1].Y - path[1].Y, fPath[p + 1].X - path[1].X);
                    }
                    else
                    {
                        al2 = al1;
                    }

                    #region LineCapStyle
                    if (_capType == DimensionLineCapType.Arrow ||
                        _capType == DimensionLineCapType.ArrowLine)
                    {
                        #region Arrow
                        disp.Draw(_lineSymbol, Arrow(path[0], l, al1));
                        disp.Draw(_lineSymbol, Arrow(path[1], -l, al1));
                        #endregion
                    }

                    if (_capType == DimensionLineCapType.ArrowLine ||
                        _capType == DimensionLineCapType.Circle ||
                        _capType == DimensionLineCapType.Line)
                    {
                        #region CapLine
                        if (p == 1 || p != fPath.PointCount - 1)
                        {
                            if (p == 1)
                            {
                                disp.Draw(_lineSymbol, CapLine(path[0], l, al1));
                            }

                            disp.Draw(_lineSymbol, CapLine(path[1], l, (al1 + al2) / 2.0));
                        }
                        if (p == fPath.PointCount - 1)
                        {
                            disp.Draw(_lineSymbol, CapLine(path[1], l, al2));
                        }
                        #endregion
                    }

                    if (_capType == DimensionLineCapType.Line)
                    {
                        #region Line
                        if (p == 1 || p != fPath.PointCount - 1)
                        {
                            if (p == 1)
                            {
                                disp.Draw(_lineSymbol, CapLine(path[0], l * 0.8, al1 - Math.PI / 4.0));
                            }

                            disp.Draw(_lineSymbol, CapLine(path[1], l * 0.8, (al1 + al2) / 2.0 - Math.PI / 4.0));
                        }
                        if (p == fPath.PointCount - 1)
                        {
                            disp.Draw(_lineSymbol, CapLine(path[1], l * 0.8, al2 - Math.PI / 4.0));
                        }
                        #endregion
                    }

                    if (_capType == DimensionLineCapType.Circle)
                    {
                        #region Circle
                        if (p == fPath.PointCount - 1)
                        {
                            disp.Draw(_lineSymbol, Circle(path[1], l * 0.8));
                        }

                        disp.Draw(_lineSymbol, Circle(path[0], l * 0.8));
                        #endregion
                    }
                    #endregion

                    #region Label
                    string label = string.Format("{0:" + _format + "}", path.Length);
                    _textSymbol.Text = label;
                    disp.Draw(_textSymbol, pLine);
                    #endregion

                    path[0].X = fPath[p].X;
                    path[0].Y = fPath[p].Y;
                }
            }
        }

        public void StartDrawing(IDisplay display) { }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
        }

        public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
        {

        }

        public bool CanRender(IFeatureLayer layer, IMap map)
        {
            return layer != null &&
                layer.FeatureClass != null &&
                layer.LayerGeometryType == GeometryType.Polyline;
        }

        public bool HasEffect(IFeatureLayer layer, IMap map)
        {
            return layer != null &&
                layer.FeatureClass != null &&
                layer.FeatureClass.GeometryType == GeometryType.Polyline;
        }

        public bool UseReferenceScale
        {
            get
            {
                return _useRefScale;
            }
            set
            {
                _useRefScale = value;
            }
        }

        public string Name
        {
            get { return "Dimension"; }
        }

        public string Category
        {
            get { return "Features"; }
        }

        public bool RequireClone()
        {
            return _textSymbol != null && _textSymbol.RequireClone() ||
                   _lineSymbol != null && _lineSymbol.RequireClone();
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            Release();

            _textSymbol = stream.Load("textSymbol", null) as ITextSymbol;
            _lineSymbol = stream.Load("lineSymbol", null) as ILineSymbol;
            _capType = (DimensionLineCapType)stream.Load("capType", DimensionLineCapType.ArrowLine);
            _format = (string)stream.Load("format", string.Empty);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("textSymbol", _textSymbol);
            stream.Save("lineSymbol", _lineSymbol);
            stream.Save("capType", _capType);
            stream.Save("format", _format);
        }

        #endregion

        #region IClone2 Member

        public object Clone(CloneOptions options)
        {
            DimensionRenderer renderer = new DimensionRenderer(options?.Display, this);
            return renderer;
        }

        public void Release()
        {
            if (_lineSymbol != null)
            {
                _lineSymbol.Release();
            }

            if (_textSymbol != null)
            {
                _textSymbol.Release();
            }

            _lineSymbol = null;
            _textSymbol = null;
        }

        #endregion

        #region ICreateDefault Member

        public ValueTask DefaultIfEmpty(object initObject)
        {
            return ValueTask.CompletedTask;
        }

        #endregion

        #region Helper
        private Polyline Arrow(IPoint s, double l, double al)
        {
            Polyline pLine = new Polyline();
            Path p = new Path();
            pLine.AddPath(p);

            p.AddPoint(new Point(s.X + l * Math.Cos(al - Math.PI / 6), s.Y + l * Math.Sin(al - Math.PI / 6)));
            p.AddPoint(new Point(s));
            p.AddPoint(new Point(s.X + l * Math.Cos(al + Math.PI / 6), s.Y + l * Math.Sin(al + Math.PI / 6)));

            return pLine;
        }
        private Polyline CapLine(IPoint s, double l, double al)
        {
            Polyline pLine = new Polyline();
            Path p = new Path();
            pLine.AddPath(p);

            p.AddPoint(new Point(s.X + l * Math.Cos(al - Math.PI / 2), s.Y + l * Math.Sin(al - Math.PI / 2)));
            p.AddPoint(new Point(s.X + l * Math.Cos(al + Math.PI / 2), s.Y + l * Math.Sin(al + Math.PI / 2)));

            return pLine;
        }
        private Polyline Circle(IPoint s, double l)
        {
            Polyline pLine = new Polyline();
            Path p = new Path();
            pLine.AddPath(p);

            double m = 2.0 * Math.PI, st = Math.PI / 8;
            for (double w = 0.0; w < m; w += st)
            {
                p.AddPoint(new Point(s.X + l * Math.Cos(w), s.Y + l * Math.Sin(w)));
            }

            p.AddPoint(new Point(p[0]));

            return pLine;
        }
        #endregion

        #region ILegendGroup Member

        public int LegendItemCount
        {
            get { return 2; }
        }

        public ILegendItem LegendItem(int index)
        {
            switch (index)
            {
                case 0:
                    _textSymbol.ShowInTOC = true;
                    return _textSymbol;
                case 1:
                    return _lineSymbol;
            }
            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (item == _textSymbol &&
                symbol != _textSymbol &&
                symbol is ITextSymbol)
            {
                _textSymbol.Release();
                _textSymbol = symbol as ITextSymbol;
            }
            else if (item == _lineSymbol &&
                symbol != _lineSymbol &&
                symbol is ILineSymbol)
            {
                _lineSymbol.Release();
                _lineSymbol = symbol as ILineSymbol;
            }
        }

        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get { return new List<ISymbol>(new ISymbol[] { _textSymbol, _lineSymbol }); }
        }

        public bool Combine(IRenderer renderer)
        {
            return false;
        }
        #endregion
    }
}
