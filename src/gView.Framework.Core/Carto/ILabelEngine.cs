using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.Common;
using System.Collections.Generic;
using gView.Framework.Core.Data;

namespace gView.Framework.Core.Carto
{
    public interface ILabelEngine
    {
        void Init(IDisplay display, bool directDraw);
        LabelAppendResult TryAppend(IDisplay display, IFeatureLayer layer, ITextSymbol symbol, IGeometry geometry, bool chechForOverlap);
        LabelAppendResult TryAppend(IDisplay display, IFeatureLayer layer, List<IAnnotationPolygonCollision> aPolygons, IGeometry geometry, bool checkForOverlap);
        void Draw(IDisplay display, ICancelTracker cancelTracker);
        void Release();

        GraphicsEngine.Abstraction.ICanvas LabelCanvas { get; }
    }
}