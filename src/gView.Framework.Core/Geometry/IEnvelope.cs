namespace gView.Framework.Core.Geometry
{
    public interface IEnvelope : IGeometry
    {
        double MinX
        {
            get;
            set;
        }

        double MinY
        {
            get;
            set;
        }

        double MaxX
        {
            get;
            set;
        }

        double MaxY
        {
            get;
            set;
        }

        IPoint LowerLeft { get; set; }
        IPoint LowerRight { get; set; }
        IPoint UpperLeft { get; set; }
        IPoint UpperRight { get; set; }
        IPoint Center { get; set; }
        IPolygon ToPolygon(int accuracy);
        IPointCollection ToPointCollection(int accuracy);

        void Union(IEnvelope envelope);

        double Width { get; }
        double Height { get; }

        void Translate(double mx, double my);
        void TranslateTo(double mx, double my);

        bool Intersects(IEnvelope envelope);
        bool Contains(IEnvelope envelope);
        bool Contains(IPoint point);

        string ToBBoxString();
    }
}
