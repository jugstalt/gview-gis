using gView.Framework.Geometry;

namespace gView.Framework.Carto
{
    public interface ISmartLabelPoint : IPoint
    {
        IMultiPoint AlernativeLabelPoints(IDisplay display);
    }
}