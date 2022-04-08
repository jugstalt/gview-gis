namespace gView.GraphicsEngine.Abstraction
{
    public interface IDisplayCharacterRanges
    {
        float Width { get; }
        CanvasRectangleF this[int i] { get; }
    }
}
