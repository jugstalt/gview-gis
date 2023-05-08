using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Abstraction
{
    public interface ISerializableExplorerObject
    {
        Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache);
    }

}
