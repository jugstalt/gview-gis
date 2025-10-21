#nullable enable

using gView.Framework.Cartography.Rendering.Exntensions;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;
using System.Collections.Generic;

namespace gView.Framework.Cartography.Rendering;

public class ExtrusionRenderer : Cloner, IFeatureRenderer2
{
    private readonly List<IFeature> _features = new();
    private ArgbColor _groundColor = ArgbColor.Gray;
    private ISymbol? _symbol = null;
    private ISymbol? _groundSymbol = null;


    #region IFeatureRenderer2

    public ISymbol? Symbol
    {
        get { return _symbol; }
        set
        {
            _symbol = value;
            _groundSymbol = value?.Clone() as ISymbol;

            if (_groundSymbol is IBrushColor brushColor)
            {
                brushColor.FillColor = _groundColor;
            }

            if (_groundSymbol is IPenColor penColor)
            {
                penColor.PenColor = _groundColor;
            }
        }
    }

    #endregion

    #region IFeatureRenderer

    public bool UseReferenceScale { get; set; }

    public string Category => "Features";

    public string Name => "Extrusion Renderer";

    public List<ISymbol> Symbols => this.Symbol is not null ? [this.Symbol] : [];

    public bool CanRender(IFeatureLayer layer, IMap map)
        => layer?.Class is IFeatureClass;

    virtual protected ExtrusionRenderer CreateCloneInstance() => new ExtrusionRenderer();

    public object Clone(CloneOptions options)
    {
        ExtrusionRenderer renderer = CreateCloneInstance();

        if (_symbol != null)
        {
            renderer.Symbol = (ISymbol)_symbol.Clone(this.UseReferenceScale ? options : null);
        }

        return renderer;
    }

    public bool Combine(IRenderer renderer) => false;

    virtual public void Draw(IDisplay disp, IFeature feature)
    {
        if (feature.Shape is not null)
        {
            _features.Add(feature);
        }
    }

    virtual public float GetElevation(IDisplay display, IFeature feature)
        => 50f;

    virtual protected void DrawGround(IDisplay disp, IFeature feature, ISymbol groundSymbol)
    {
        disp.Canvas.SmoothingMode = SmoothingMode.AntiAlias;
        disp.Draw(_groundSymbol, feature.Shape);
        disp.Canvas.SmoothingMode = SmoothingMode.None;
    }

    virtual protected void DrawElevated(IDisplay disp, IFeature feature, ISymbol symbol, IGeometry elevatedGeometry)
    {
        disp.Draw(_symbol, elevatedGeometry);
    }

    public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
    {
        #region Draw Ground

        if (_groundSymbol is not null)
        {
            foreach (var feature in _features)
            {
                DrawGround(disp, feature, _groundSymbol);
            }
        }

        #endregion

        #region Draw Elevated 

        if (_symbol is not null)
        {
            foreach (var feature in _features)
            {
                var geometry = disp.GeometricTransformer.Transform2D(feature.Shape) as IGeometry;
                geometry = disp.GeometricTransformer.InvTransform2D(
                                        geometry.Elevate(disp, GetElevation(disp, feature))
                                    ) as IGeometry;
                if (geometry is not null)
                {
                    DrawElevated(disp, feature, _symbol, geometry);
                }
            }
        }

        #endregion

        _features.Clear();
    }

    public bool HasEffect(IFeatureLayer layer, IMap map)
        => layer?.Class is IFeatureClass && _symbol != null;

    virtual public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
    {

    }

    public void Release()
    {
        _groundSymbol?.Release();
        _groundSymbol = null;

        _symbol?.Release();
        _symbol = null;
    }

    public bool RequireClone() => true;

    public void Load(IPersistStream stream)
    {
        this.Symbol = (ISymbol)stream.Load("Symbol");
    }

    public void Save(IPersistStream stream)
    {
        if (this.Symbol != null)
        {
            stream.Save("Symbol", this.Symbol);
        }
    }

    public void StartDrawing(IDisplay display)
    {
        _features.Clear();
    }

    #endregion
}
