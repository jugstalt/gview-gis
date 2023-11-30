using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using System.Collections.Generic;

namespace gView.Framework.Core.Symbology
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
