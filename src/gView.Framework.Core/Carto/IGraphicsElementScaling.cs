namespace gView.Framework.Core.Carto
{
    public interface IGraphicsElementScaling
    {
        void Scale(double scaleX, double scaleY);
        void ScaleX(double scale);
        void ScaleY(double scale);
    }
}