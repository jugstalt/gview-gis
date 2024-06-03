using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Geometry;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System;
using System.ComponentModel;

namespace gView.Framework.Symbology;

[RegisterPlugIn("C35A7287-AA51-41EC-8DC5-A0C41E98C18B")]
public class ExtrusionFillSymbol : LegendItem, 
                                   IFillSymbol, 
                                   IBrushColor
{
    private IBrush _brush, _groundBrush;

    public ExtrusionFillSymbol()
    {
        _brush = Current.Engine.CreateSolidBrush(ArgbColor.White);
        _groundBrush = Current.Engine.CreateSolidBrush(ArgbColor.Gray);
    }

    private ExtrusionFillSymbol(ArgbColor color)
    {
        _brush = Current.Engine.CreateSolidBrush(color);
        _groundBrush = Current.Engine.CreateSolidBrush(ArgbColor.Gray);
    }

    ~ExtrusionFillSymbol()
    {
        this.Release();
    }

    [Browsable(true)]
    [Category("Fill Symbol")]
    [UseColorPicker()]
    public ArgbColor Color
    {
        get
        {
            return _brush.Color;
        }
        set
        {
            _brush.Color = value;
        }
    }

    #region IBrushColor

    public ArgbColor FillColor
    {
        get => this.Color;
        set => this.Color = value;
    }

    #endregion

    [Browsable(true)]
    public double Elevation { get; set; }

    public string Name => "Extrusion Fill Symbol";

    public SymbolSmoothing SymbolSmoothingMode { get; set; } = SymbolSmoothing.AntiAlias;

    public object Clone(CloneOptions options)
    {
        var clone = new ExtrusionFillSymbol(_brush.Color);
        
        clone.LegendLabel = _legendLabel;
        clone.Elevation = Elevation;

        return clone;
    }

    public void Draw(IDisplay display, IGeometry geometry)
    {
        display.Canvas.SmoothingMode = SymbolSmoothingMode switch
        {
            SymbolSmoothing.AntiAlias => SmoothingMode.AntiAlias,
            _ => SmoothingMode.Default,
        };

        var gp = DisplayOperations.Geometry2GraphicsPath(display, geometry);
        if (gp != null)
        {
            this.FillPath(display, gp, _groundBrush);

            gp.Dispose(); gp = null;
        }

        var elevatedGeometry = ElevateGeometry(display, geometry, this.Elevation);

        var gpElevated = DisplayOperations.Geometry2GraphicsPath(display, elevatedGeometry);
        if (gpElevated != null)
        {
            this.FillPath(display, gpElevated, _brush);

            gpElevated.Dispose(); gpElevated = null;
        }

        display.Canvas.SmoothingMode = SmoothingMode.None;
    }

    public void FillPath(IDisplay display, IGraphicsPath path)
    {
        if (!_brush.Color.IsTransparent)
        {
            display.Canvas.FillPath(_brush, path);
        }
    }

    private void FillPath(IDisplay display, IGraphicsPath path, IBrush brush)
    {
        display.Canvas.FillPath(brush, path);
    }

    #region IPersistable Member

    new public void Load(IPersistStream stream)
    {
        base.Load(stream);

        this.Color = ArgbColor.FromArgb((int)stream.Load("color", ArgbColor.White.ToArgb()));
        this.Elevation = (double)stream.Load("elevation", this.Elevation);
    }

    new public void Save(IPersistStream stream)
    {
        base.Save(stream);

        stream.Save("color", this.Color.ToArgb());
        stream.Save("elevation", this.Elevation);
    }

    #endregion

    public void Release()
    {
        if (_brush != null)
        {
            _brush.Dispose();
            _brush = null;
        }

        if(_groundBrush != null)
        {
            _groundBrush.Dispose();
            _groundBrush = null;
        }
    }

    public bool RequireClone() => true;

    public bool SupportsGeometryType(GeometryType type) => type == GeometryType.Polygon;

    #region Helper

    private IGeometry ElevateGeometry(IDisplay display, IGeometry geometry, double elevation)
    {
        var Z = display.MapScale;
        if (Z <= elevation) return null;

        var center = display.Envelope.Center;

        if (geometry is IPolygon polygon)
        {
            var elevatedPolygon = new Polygon();

            for (int r = 0; r < polygon.RingCount; r++)
            {
                var ring = polygon[r];

                var elevatedRing = new Ring();
                elevatedPolygon.AddRing(elevatedRing);

                for (var p = 0; p < ring.PointCount; p++)
                {
                    elevatedRing.AddPoint(ElevatePoint(ring[p], center, Z, elevation));
                }
            }

            return elevatedPolygon;
        }

        return null;
    }

    private IPoint ElevatePoint(IPoint point, IPoint center, double Z, double elevation)
    {
        double dx = point.X - center.X;
        double dy = point.Y - center.Y;

        double d = Math.Sqrt(dx * dx + dy * dy);
        double t = d / Z * elevation;

        double rx = dx / d, ry = dy / d;

        return new Point(point.X + rx * t, point.Y + ry * t);
    }

    #endregion
}
