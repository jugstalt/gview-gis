using System.Threading.Tasks;

namespace gView.Framework.Core.UI
{
    public interface IDefault
    {
        ValueTask DefaultIfEmpty(object initObject);
    }
}