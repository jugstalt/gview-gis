using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    public interface IImageDB
    {
        Task<int> CreateImageDataset(string name, ISpatialReference sRef, ISpatialIndexDef sIndexDef, string imageSpace, IFieldCollection fields);
        Task<(bool isImageDataset, string imageSpace)> IsImageDataset(string dsname);
    }
}
