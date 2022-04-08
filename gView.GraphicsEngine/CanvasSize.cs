namespace gView.GraphicsEngine
{
    public struct CanvasSize
    {
        public CanvasSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
