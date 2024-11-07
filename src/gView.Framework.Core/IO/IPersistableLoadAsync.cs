using System.Threading.Tasks;

namespace gView.Framework.Core.IO
{
    public interface IPersistableLoadAsync
    {
        Task<bool> LoadAsync(IPersistStream stream);
        void Save(IPersistStream stream);
    }
}
