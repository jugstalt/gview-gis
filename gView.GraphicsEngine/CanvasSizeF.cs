namespace gView.GraphicsEngine
{
    public struct CanvasSizeF
    {
        public CanvasSizeF(float width, float height)
        {
            this.Width = width;
            this.Height = height;
        }

        public float Width { get; set; }
        public float Height { get; set; }
    }
}
