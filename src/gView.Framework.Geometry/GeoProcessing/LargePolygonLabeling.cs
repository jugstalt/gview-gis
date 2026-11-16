using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Engine;

namespace gView.Framework.Geometry.GeoProcessing
{
    /// <summary>
    /// Clipping and label points for polygons with very many vertices, where the classic algorithms
    /// (Clipper, ring scan lines) are too slow: done with SpatialNET.
    /// </summary>
    public static class LargePolygonLabeling
    {
        /// <summary>The part of <paramref name="polygon"/> inside <paramref name="envelope"/> (an empty polygon if none).</summary>
        public static IPolygon Clip(IPolygon polygon, IEnvelope envelope)
            => SpatialNetBridge.ClipPolygon(polygon, envelope);

        /// <summary>
        /// Label points of <paramref name="polygon"/>: one per part if <paramref name="perPart"/>
        /// (needs shells and holes typed and grouped, as <see cref="Clip"/> gives them), otherwise one for the whole polygon. <paramref name="pixelSize"/> is the size of one pixel in map units
        /// (the alternative points of the result are 20 pixels apart).
        /// <paramref name="visible"/> (optional): alternative points outside of it are skipped.
        /// </summary>
        public static IMultiPoint LabelPoints(IPolygon polygon, double pixelSize, bool perPart, IEnvelope visible = null)
            => SpatialNetBridge.LabelPoints(polygon, pixelSize, perPart, visible);
    }
}
