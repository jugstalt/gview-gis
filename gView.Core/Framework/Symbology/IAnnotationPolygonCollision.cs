namespace gView.Framework.Symbology
{
    public interface IAnnotationPolygonCollision
    {
        bool CheckCollision(IAnnotationPolygonCollision poly);
        bool Contains(float x, float y);
        AnnotationPolygonEnvelope Envelope { get; }
    }
}
