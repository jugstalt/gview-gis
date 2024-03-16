using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Vtc;

namespace gView.Framework.Cartography.Rendering.Vtc;

public class VtcFeatureRenderer : IFeatureRenderer
{
    private readonly PaintSymbol _paintSymbol;

    public VtcFeatureRenderer(PaintSymbol paintSymbol)
    {
        _paintSymbol = paintSymbol;
    }

    public bool UseReferenceScale { get; set; } = false;

    public string Category => "vtc";

    public string Name => "Vector Tile Cache Renderer";

    public List<ISymbol> Symbols => [ _paintSymbol ];

    public bool CanRender(IFeatureLayer layer, IMap map)
    {
        return true;
    }

    public object Clone() => new VtcFeatureRenderer((PaintSymbol)_paintSymbol.Clone());
 

    public object Clone(CloneOptions options) => new VtcFeatureRenderer((PaintSymbol)_paintSymbol.Clone(options));

    public bool Combine(IRenderer renderer)
    {
        return false;
    }

    public void Draw(IDisplay disp, IFeature feature)
    {
        _paintSymbol.Draw(disp, feature);
    }

    public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
    {
        
    }

    public bool HasEffect(IFeatureLayer layer, IMap map) => true;

    public void Load(IPersistStream stream)
    {
        
    }

    public void Save(IPersistStream stream)
    {

    }

    public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
    {
        
    }

    public void Release()
    {
        _paintSymbol.Release();
    }

    public bool RequireClone() => _paintSymbol.RequireClone();
    
    public void StartDrawing(IDisplay display)
    {

    }
}
