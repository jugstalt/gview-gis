using gView.Framework.Data;
using System.Threading.Tasks;

namespace gView.Framework.FDB
{
    public interface IAltertable
    {
        Task<bool> AlterTable(string table, IField oldField, IField newField);
    }
}
