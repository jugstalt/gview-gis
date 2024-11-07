using gView.Framework.Core.MapServer;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IRequestDependentDataset
    {
        Task<bool> Open(IServiceRequestContext context);
    }
}