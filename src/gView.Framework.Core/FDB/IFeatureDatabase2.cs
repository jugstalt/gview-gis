using gView.Framework.Core.Geometry;
using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    public interface IFeatureDatabase2 : IFeatureDatabase
    {
        Task<int> CreateDataset(string name, ISpatialReference sRef, ISpatialIndexDef sIndexDef);
    }
}
