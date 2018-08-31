using System;
using System.Collections.Generic;

namespace gView.Framework.Data
{
    public interface IDBOperations
    {
        bool BeforeInsert(ITableClass tClass);
        bool BeforeUpdate(ITableClass tClass);
        bool BeforeDelete(ITableClass tClass);
    }

    public interface IOID
    {
        /// <summary>
        /// The object ID of the object.
        /// </summary>
        int OID { get; }
    }
    public interface IGlobalOID
    {
        long GlobalOID { get; }
    }

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

    public interface IRow : IOID, IRowData
    {
    }
    public interface IGlobalRow : IGlobalOID, IRowData
    {
    }

    /// <summary>
    /// Provide access to members an properties that hand out enumerated elements.
    /// </summary>
    public interface ICursor : IDisposable
    {

    }

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
        IRow NextRow { get; }
    }

    /// <summary>
    /// Provide access to members an properties that hand out enumerated features.
    /// </summary>
    public interface IFeatureCursor : ICursor
    {
        // <summary>
        /// Advance the position of the cursor by one and return the Feature object at that position.
        /// </summary>
        /// /// <remarks>
        /// Return <c>null</c> after the last row is reached.
        /// </remarks>
        IFeature NextFeature { get; }
    }

    public interface IFeatureCursorSkills : IFeatureCursor
    {
        bool KnowsFunctions { get; }
    }

    public interface ITextCursor : ICursor
    {
        string Text { get; }
    }
    public interface IUrlCursor : ICursor
    {
        string Url { get; }
    }
    public interface IXmlCursor : ICursor
    {
        string Xml { get; }
    }
    //public enum getFeatureQueryType { Geometry, Attributes, All }

    //public enum FieldDomainType { Range, Values, Lookup }
    public interface IFieldDomain : gView.Framework.IO.IPersistable, gView.Framework.system.IClone
    {
        string Name { get; }
    }
    public interface IRangeFieldDomain : IFieldDomain
    {
        double MinValue { get; }
        double MaxValue { get; }
    }
    public interface IValuesFieldDomain : IFieldDomain
    {
        object[] Values { get; }
    }
    public interface IDictionaryFieldDomain : IFieldDomain
    {
        Dictionary<string /* tag */, object /* dbValue */> Dictionary { get; }
    }

    public enum FieldType
    {
        ID = 0,
        Shape = 1,
        boolean = 2,
        biginteger = 3,
        character = 4,
        integer = 5,
        smallinteger = 6,
        Float = 7,
        Double = 8,
        String = 9,
        Date = 10,
        unknown = 11,
        binary = 12,
        guid = 13,
        replicationID = 14,
        GEOMETRY = 15,
        GEOGRAPHY = 16,
        NString = 17
    }

    public enum GeometryFieldType
    {
        Default = FieldType.Shape,
        MsGeometry = FieldType.GEOMETRY,
        MsGeography = FieldType.GEOGRAPHY
    }

    public interface IField : gView.Framework.IO.IMetadata
    {
        string name
        {
            get;
        }

        string aliasname
        {
            get;
        }
        int precision
        {
            get;
        }
        int size
        {
            get;
        }
        FieldType type
        {
            get;
        }

        bool visible { get; set; }
        bool IsRequired { get; }
        bool IsEditable { get; }
        object DefautValue { get; }

        IFieldDomain Domain { get; }
    }

    public interface IFieldValue
    {
        string Name { get; }
        object Value { get; set; }
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
