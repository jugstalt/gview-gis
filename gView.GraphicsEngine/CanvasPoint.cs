namespace gView.GraphicsEngine
{
    public struct CanvasPoint
    {
        public CanvasPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public CanvasPoint(CanvasPoint point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}
