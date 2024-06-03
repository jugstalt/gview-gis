using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Data
{
    public interface IFeatureTable : ITable
    {
        IGeometry Shape(object ObjectID);
    }
}