using System.Globalization;
using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using Path = gView.Framework.Geometry.Path;

namespace gView.Framework.Cartography.Rendering.Tests;

/// <summary>
/// Tests for <see cref="SimpleLabelRenderer.Draw"/>'s support of the reserved
/// <c>[$feature.length]</c>/<c>[$feature.area]</c> placeholders - resolved from the feature's
/// actual geometry rather than from an attribute field (see
/// <c>AprxLabelExpressionParser</c>'s <c>Length($feature)</c>/<c>Area($feature)</c> conversion).
/// </summary>
public class SimpleLabelRendererGeometryExpressionTests
{
    // A renderer/feature combination whose Draw() call, for a polyline shape, sets the label text
    // and then returns before ever touching the label engine/canvas (PrepareQueryFilter is never
    // called here, so _clipEnvelope stays null, Clip.PerformClip(null, ...) returns null, and Draw
    // returns right after) - the substitution logic under test runs unconditionally before that
    // point, so no display/label-engine setup is needed. IFeatureLayer is never dereferenced on
    // this path either, so passing null for it is safe here.
    private static string DrawAndGetText(string expression, IGeometry shape, params (string Name, object Value)[] fields)
    {
        var renderer = new SimpleLabelRenderer
        {
            UseExpression = true,
            LabelExpression = expression,
            TextSymbol = new SimpleTextSymbol(),
        };

        var feature = new Feature { Shape = shape };
        foreach (var (name, value) in fields)
        {
            feature.Fields.Add(new FieldValue(name, value));
        }

        var display = new Display(null);

        renderer.Draw(display, null!, feature);

        return renderer.TextSymbol.Text;
    }

    private static Polyline Line(double x1, double y1, double x2, double y2) =>
        new(new Path(new List<IPoint> { new Point(x1, y1), new Point(x2, y2) }));

    // -----------------------------------------------------------------------
    // [$feature.length]
    // -----------------------------------------------------------------------

    [Fact]
    public void ShapeFeatureLength_Polyline_ResolvesToComputedLength()
    {
        var text = DrawAndGetText("[$feature.length]", Line(0, 0, 3, 4)); // 3-4-5 triangle -> 5

        Assert.Equal("5", text);
    }

    [Fact]
    public void ShapeFeatureLength_WithFormat_AppliesFormat()
    {
        var text = DrawAndGetText("[$feature.length:F2]", Line(0, 0, 1, 1)); // sqrt(2) = 1.4142...

        // ExpressionExtensions.EvaluateExpression formats with CultureInfo.CurrentCulture (matches
        // real deployment behaviour, e.g. a German-locale server using "," as decimal separator) -
        // so the expected text is derived the same way rather than hard-coded to one locale.
        Assert.Equal(Math.Sqrt(2).ToString("F2", CultureInfo.CurrentCulture), text);
    }

    [Fact]
    public void ShapeFeatureLength_PointGeometry_ResolvesToZero()
    {
        var text = DrawAndGetText("[$feature.length]", new Point(5, 5));

        Assert.Equal("0", text);
    }

    [Fact]
    public void ShapeFeatureLength_CombinedWithRealField_BothResolveIndependently()
    {
        var text = DrawAndGetText("[NAME]: [$feature.length]", Line(0, 0, 3, 4), ("NAME", "Segment A"));

        Assert.Equal("Segment A: 5", text);
    }

    // -----------------------------------------------------------------------
    // [$feature.area]
    // -----------------------------------------------------------------------

    [Fact]
    public void ShapeFeatureArea_Polygon_ResolvesToComputedArea()
    {
        var ring = new Ring(new List<IPoint>
        {
            new Point(0, 0), new Point(10, 0), new Point(10, 10), new Point(0, 10), new Point(0, 0),
        });
        var polygon = new Polygon(ring);

        var text = DrawAndGetText("[$feature.area]", polygon);

        Assert.Equal("100", text);
    }

    // -----------------------------------------------------------------------
    // Real "Shape_Length" attribute field - sanity check that it's completely unrelated to (and
    // unaffected by) the new "[$feature.length]" pseudo-field logic.
    // -----------------------------------------------------------------------

    [Fact]
    public void RealShapeLengthField_StillWorksAsAnOrdinaryField()
    {
        var text = DrawAndGetText("[Shape_Length]", Line(0, 0, 3, 4), ("Shape_Length", 42.0));

        Assert.Equal("42", text);
    }
}
