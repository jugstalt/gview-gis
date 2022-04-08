using System.Collections.Generic;
using gView.Framework.Geometry;
using gView.Framework.Carto;

namespace gView.Framework.Symbology
{
    public interface ILabel
    {
        string Text { get; set; }
        TextSymbolAlignment TextSymbolAlignment { get; set; }

        TextSymbolAlignment[] SecondaryTextSymbolAlignments { get; set; }

        GraphicsEngine.Abstraction.IDisplayCharacterRanges MeasureCharacterWidth(IDisplay display);

        List<IAnnotationPolygonCollision> AnnotationPolygon(IDisplay display, IGeometry geometry, TextSymbolAlignment symbolAlignment);
    }
}
