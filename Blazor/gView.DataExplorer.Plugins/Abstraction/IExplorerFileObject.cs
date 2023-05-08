using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.Abstraction
{
    public interface IExplorerFileObject : IExplorerObject
    {
        string Filter { get; }
        Task<IExplorerFileObject> CreateInstance(IExplorerObject parent, string filename);
    }

}
