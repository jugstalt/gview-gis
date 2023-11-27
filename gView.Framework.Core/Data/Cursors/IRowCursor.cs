using System.Threading.Tasks;
using gView.Framework.Core.Data;

namespace gView.Framework.Core.Data.Cursors
{
    /// <summary>
    /// Provide access to members an properties that hand out enumerated database rows.
    /// </summary>
    public interface IRowCursor : ICursor
    {
        /// <summary>
        /// Advance the position of the cursor by one and return the Row object at that position.
        /// </summary>
        /// <remarks>
        /// Return <c>null</c> after the last row is reached.
        /// </remarks>
        Task<IRow> NextRow();
    }



    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
