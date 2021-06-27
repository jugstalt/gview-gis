using gView.GraphicsEngine.Generics;

namespace gView.GraphicsEngine
{
    public class CanvasPointF : CanvasPointGeneric<float>
    {
        public CanvasPointF()
            : base(0f, 0f)
        { }

        public CanvasPointF(float x, float y)
            : base(x, y)
        { }

        public CanvasPointF(CanvasPointF point)
            : base(point?.X ?? 0f, point?.Y ?? 0f)
        { }
    }
}
