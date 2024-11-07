namespace gView.GraphicsEngine
{
    public struct CanvasPointF
    {
        public CanvasPointF(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public CanvasPointF(CanvasPointF point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public float X { get; set; }
        public float Y { get; set; }
    }
}
