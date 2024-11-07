using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface ITable
    {
        event RowsAddedToTableEvent RowsAddedToTable;

        Task<int> Fill();
        Task<int> Fill(int next_N_Rows);
        Task<int> FillAtLeast(List<int> IDs);

        bool hasMore { get; }

        DataTable Table { get; }
        string IDFieldName { get; }
    }
}