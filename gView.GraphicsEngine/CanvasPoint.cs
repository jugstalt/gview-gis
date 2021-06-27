using gView.GraphicsEngine.Generics;

namespace gView.GraphicsEngine
{
    public class CanvasPoint : CanvasPointGeneric<int>
    {
        public CanvasPoint()
            : base(0, 0)
        { }

        public CanvasPoint(int x, int y)
            : base(x, y)
        { }

        public CanvasPoint(CanvasPoint point)
            : base(point?.X ?? 0, point?.Y ?? 0)
        { }
    }
}
