using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Carto
{
    public interface ISmartLabelPoint : IPoint
    {
        IMultiPoint AlernativeLabelPoints(IDisplay display);
    }
}