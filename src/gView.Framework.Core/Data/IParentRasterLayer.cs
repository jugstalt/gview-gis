using gView.Framework.Core.Carto;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IParentRasterLayer
    {
        Task<IRasterLayerCursor> ChildLayers(IDisplay display, string filterClause);
    }
}