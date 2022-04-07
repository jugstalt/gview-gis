namespace gView.Framework.Geometry
{
    public interface IDisplayPath : IGeometry
    {
        float Chainage { get; set; }

        gView.Framework.Symbology.IAnnotationPolygonCollision AnnotationPolygonCollision { get; set; }

        float Length { get; }
        GraphicsEngine.CanvasPointF? PointAt(float stat);

        void ChangeDirection();
    }
}
