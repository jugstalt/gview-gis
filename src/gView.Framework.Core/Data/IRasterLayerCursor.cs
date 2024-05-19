using gView.Framework.Core.Data.Cursors;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IRasterLayerCursor : ICursor
    {
        Task<IRasterLayer> NextRasterLayer();
    }
}