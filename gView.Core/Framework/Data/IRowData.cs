using System.Collections.Generic;

namespace gView.Framework.Data
{
    /// <summary>
    /// Provides access to members and properties that return information about a database row.
    /// </summary>
    public interface IRowData : IDBOperations
    {
        /// <summary>
        /// The fields of the row.
        /// </summary>
        /// <remarks>
        /// Each element of the array represents a <see cref="gView.Framework.IFieldValue"/>.
        /// </remarks>
        List<FieldValue> Fields { get; }

        object this[string fieldname] { get; set; }
        object this[int fieldIndex] { get; set; }

        FieldValue FindField(string name);

        bool CaseSensitivFieldnameMatching { get; set; }
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
