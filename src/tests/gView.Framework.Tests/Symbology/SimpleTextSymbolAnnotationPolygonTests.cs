using System.Linq;
using gView.Framework.Cartography;
using gView.Framework.Core.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Framework.Tests.Symbology;

/// <summary>
/// Regression tests for a bug in <see cref="SimpleTextSymbol.AnnotationPolygon"/> (the collision
/// box the label engine's overlap check runs against): it used to compute that box's rotation
/// from the <c>Angle</c> property alone, while the actual drawing (<c>DrawAtPoint</c>) rotates by
/// <c>angle-parameter + Angle + Rotation</c> (the latter being <see cref="ISymbolRotation.Rotation"/>
/// - e.g. a per-feature rotation field). For point/multipoint geometry this meant a
/// rotation-field-driven label's collision box stayed un-rotated. For polyline geometry it was
/// worse: the line segment's own direction angle was computed and then discarded entirely, so a
/// diagonal line label's box stayed axis-aligned no matter how steep the line ran. In both cases
/// the label was drawn correctly rotated but checked for overlap against the wrong shape, letting
/// rotated labels silently overlap their neighbours.
/// </summary>
public class SimpleTextSymbolAnnotationPolygonTests
{
    private static Display NewDisplay(int size = 200)
    {
        var display = new Display(null);
        display.Dpi = Current.Engine.ScreenDpi;

        var env = new Envelope(0, 0, size, size);
        display.Limit = env;
        display.ImageWidth = size;
        display.ImageHeight = size;
        display.ZoomTo(env.MinX, env.MinY, env.MaxX, env.MaxY);

        display.Canvas = Current.Engine.CreateBitmap(size, size).CreateCanvas();

        return display;
    }

    private static SimpleTextSymbol NewSymbol(float rotation = 0f) => new()
    {
        Text = "Hello",
        Font = Current.Engine.CreateFont("Arial", 12f),
        Rotation = rotation
    };

    [Fact]
    public void PointGeometry_RotationProperty_RotatesTheCollisionBox()
    {
        var display = NewDisplay();
        var point = new Point(100, 100);

        var unrotatedEnv = NewSymbol(rotation: 0f)
            .AnnotationPolygon(display, point, TextSymbolAlignment.Center)!
            .Single().Envelope;
        var rotatedEnv = NewSymbol(rotation: 90f)
            .AnnotationPolygon(display, point, TextSymbolAlignment.Center)!
            .Single().Envelope;

        var unrotatedWidth = unrotatedEnv.MaxX - unrotatedEnv.MinX;
        var unrotatedHeight = unrotatedEnv.MaxY - unrotatedEnv.MinY;
        var rotatedWidth = rotatedEnv.MaxX - rotatedEnv.MinX;
        var rotatedHeight = rotatedEnv.MaxY - rotatedEnv.MinY;

        Assert.True(unrotatedWidth > unrotatedHeight, "sanity: unrotated text should read wider than tall");
        // A 90° rotation must swap which dimension is larger - before the fix, Rotation was
        // ignored entirely and rotatedEnv came out identical to unrotatedEnv.
        Assert.True(rotatedHeight > rotatedWidth, "a 90°-rotated label's collision box must come out taller than wide");
    }

    [Fact]
    public void PolylineGeometry_DiagonalSegment_RotatesCollisionBoxToFollowTheLine()
    {
        var display = NewDisplay();

        var horizontalLine = new Polyline();
        var hPath = new gView.Framework.Geometry.Path();
        hPath.AddPoint(new Point(20, 100));
        hPath.AddPoint(new Point(180, 100));
        horizontalLine.AddPath(hPath);

        var diagonalLine = new Polyline();
        var dPath = new gView.Framework.Geometry.Path();
        dPath.AddPoint(new Point(20, 20));
        dPath.AddPoint(new Point(180, 180));
        diagonalLine.AddPath(dPath);

        var hEnv = NewSymbol()
            .AnnotationPolygon(display, horizontalLine, TextSymbolAlignment.Center)!
            .Single().Envelope;
        var dEnv = NewSymbol()
            .AnnotationPolygon(display, diagonalLine, TextSymbolAlignment.Center)!
            .Single().Envelope;

        var hHeight = hEnv.MaxY - hEnv.MinY;
        var dHeight = dEnv.MaxY - dEnv.MinY;

        // A rotated rectangle's axis-aligned bounding box is never smaller than the unrotated
        // one's, and grows a lot for a 45° tilt. Before the fix, the computed segment angle was
        // discarded and dEnv came out identical to a hypothetical horizontal placement (same
        // width/height as hEnv) regardless of the line's actual slope.
        Assert.True(dHeight > hHeight * 1.5, "a 45°-diagonal line label's box must be visibly taller once actually rotated to follow the line");
    }
}
