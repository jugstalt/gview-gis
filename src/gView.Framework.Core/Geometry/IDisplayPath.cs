using gView.Framework.Core.Symbology;

namespace gView.Framework.Core.Geometry
{
    public interface IDisplayPath : IGeometry
    {
        float Chainage { get; set; }

        IAnnotationPolygonCollision AnnotationPolygonCollision { get; set; }

        float Length { get; }
        GraphicsEngine.CanvasPointF? PointAt(float stat);

        void ChangeDirection();
    }
}
