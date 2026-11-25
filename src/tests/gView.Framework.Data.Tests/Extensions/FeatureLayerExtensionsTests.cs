using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;

namespace gView.Framework.Data.Tests.Extensions;

public class FeatureLayerExtensionsTests
{
    [Fact]
    public void CheckRequestRendererFields_KnownFields_DoesNotThrow()
    {
        var fc = FakeFeatureClass.Default();
        var layer = new FeatureLayer(fc) { FeatureRenderer = new FieldsRenderer("NAME", "population") };

        layer.CheckRequestRendererFields(fc);
    }

    [Theory]
    [InlineData("PASSWORD")]
    [InlineData("(select password from users)")]
    [InlineData("NAME,PASSWORD")]
    public void CheckRequestRendererFields_UnknownField_Throws(string field)
    {
        var fc = FakeFeatureClass.Default();
        var layer = new FeatureLayer(fc) { FeatureRenderer = new FieldsRenderer("NAME", field) };

        Assert.Throws<SqlDangerousStatementExceptions>(() => layer.CheckRequestRendererFields(fc));
    }

    [Fact]
    public void CheckRequestRendererFields_NoRenderer_DoesNotThrow()
    {
        var fc = FakeFeatureClass.Default();

        new FeatureLayer(fc).CheckRequestRendererFields(fc);
    }

    /// <summary>
    /// A renderer (e.g. from a request: SLD, dynamicLayers) that adds the given fields to the query.
    /// </summary>
    private class FieldsRenderer(params string[] fields) : IFeatureRenderer
    {
        public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
        {
            foreach (var field in fields)
            {
                filter.AddField(field);
            }
        }

        public void Draw(IDisplay disp, IFeature feature) { }
        public void StartDrawing(IDisplay display) { }
        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker) { }
        public bool HasEffect(IFeatureLayer layer, IMap map) => true;
        public bool UseReferenceScale { get; set; }
        public string Category => "test";
        public bool RequireClone() => false;
        public string Name => "fields";
        public List<ISymbol> Symbols => [];
        public bool Combine(IRenderer renderer) => false;
        public bool CanRender(IFeatureLayer layer, IMap map) => true;
        public object Clone() => this;
        public object Clone(CloneOptions options) => this;
        public void Release() { }
        public void Load(IPersistStream stream) { }
        public void Save(IPersistStream stream) { }
    }
}
