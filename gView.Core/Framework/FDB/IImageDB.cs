using gView.Framework.Data;
using gView.Framework.Geometry;
using System.Threading.Tasks;

namespace gView.Framework.FDB
{
    public interface IImageDB
    {
        Task<int> CreateImageDataset(string name, ISpatialReference sRef, ISpatialIndexDef sIndexDef, string imageSpace, IFields fields);
        Task<(bool isImageDataset, string imageSpace)> IsImageDataset(string dsname);
    }
}
