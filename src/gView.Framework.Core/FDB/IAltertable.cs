using gView.Framework.Core.Data;
using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    public interface IAltertable
    {
        Task<bool> AlterTable(string table, IField oldField, IField newField);
    }
}
