using System.Threading.Tasks;

namespace gView.Framework.IO
{
    public interface IMetadataProvider : IPersistable
    {
        Task<bool> ApplyTo(object Object);
        string Name { get; }
    }
}
