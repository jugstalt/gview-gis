using gView.Framework.Geometry;
using System.Threading.Tasks;

namespace gView.Framework.FDB
{
    public interface IFeatureDatabase2 : IFeatureDatabase
    {
        Task<int> CreateDataset(string name, ISpatialReference sRef, ISpatialIndexDef sIndexDef);
    }
}
