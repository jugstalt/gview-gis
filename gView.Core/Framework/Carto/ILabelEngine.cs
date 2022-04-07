using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.system;
using System.Collections.Generic;

namespace gView.Framework.Carto
{
    public interface ILabelEngine
    {
        void Init(IDisplay display, bool directDraw);
        LabelAppendResult TryAppend(IDisplay display, ITextSymbol symbol, IGeometry geometry, bool chechForOverlap);
        LabelAppendResult TryAppend(IDisplay display, List<IAnnotationPolygonCollision> aPolygons, IGeometry geometry, bool checkForOverlap);
        void Draw(IDisplay display, ICancelTracker cancelTracker);
        void Release();

        GraphicsEngine.Abstraction.ICanvas LabelCanvas { get; }
    }
}