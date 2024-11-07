using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IDataset2 : IDataset
    {
        Task<IDataset2> EmptyCopy();
        Task AppendElement(string elementName);
    }
}