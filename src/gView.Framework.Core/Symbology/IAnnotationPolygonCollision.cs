namespace gView.Framework.Core.Symbology
{
    public interface IAnnotationPolygonCollision
    {
        bool CheckCollision(IAnnotationPolygonCollision poly);
        bool Contains(float x, float y);
        AnnotationPolygonEnvelope Envelope { get; }
        IAnnotationPolygonCollision WithSpacing(SymbolSpacingType type, float spacingX, float spacingY);
    }
}
