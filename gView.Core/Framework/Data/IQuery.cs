using System;
using System.Collections.Generic;
using gView.Framework.Geometry;
using gView.Framework.system;

namespace gView.Framework.Data
{
	/// <summary>
	/// Zusammenfassung für IQuery.
	/// </summary>

	public interface IQueryFilter : ICloneable,IClone4,IUserData
	{
        void AddField(string fieldname, bool caseSensitive = true);
        void AddField(string fieldname, string alias);
        string Alias(string fieldname);

		string SubFields { get ; set ; }
        string SubFieldsAndAlias { get; }
		string WhereClause { set ; get ; }
        string JsonWhereClause { get; }
		string OrderBy { set; get; }
        bool NoLock { set; get; }
		int BeginRecord { get ; set ;}
		int LastQueryFeatureCount { get ; }
		bool HasMore { get ; }

		string fieldPrefix { get ; set ; }
		string fieldPostfix { get ; set ; }

        ISpatialReference FeatureSpatialReference { get; set; }
        ISpatialReference ContextLayerDefaultSpatialReference { get; set; }

        bool IgnoreUndefinedFields { get; set; }
	}

    public interface IDistinctFilter : IQueryFilter
    {
    }

    public interface IFunctionFilter : IQueryFilter
    {
        string Function { get; }
        string Alias { get; }
    }

    public interface IRowIDFilter : IQueryFilter
    {
        List<int> IDs { get; set; }
        string RowIDWhereClause { get; }
        string IdFieldName { get; }
    }

    public interface IGlobalRowIDFilter : IQueryFilter
    {
        List<long> IDs { get; set; }
        string RowIDWhereClause { get; }
    }

    public enum spatialRelation
    {
        SpatialRelationMapEnvelopeIntersects = 0,
        SpatialRelationIntersects = 1,
        SpatialRelationEnvelopeIntersects = 2,
        SpatialRelationWithin = 3,
        SpatialRelationContains = 4
    }

	public interface ISpatialFilter : IQueryFilter 
	{
		IGeometry Geometry { get ; set ; }
        //IGeometry GeometryEx { get; }
        ISpatialReference FilterSpatialReference { get; set; }
		//double BufferDistance { get; set ; }
		//bool FuzzyQuery { get ; set ; }
        spatialRelation SpatialRelation { get; set; }
	}

    public interface IBufferQueryFilter : IQueryFilter
    {
        IQueryFilter RootFilter { get; }
        IFeatureClass RootFeatureClass { get; }

        double BufferDistance { get; }
        gView.Framework.Carto.GeoUnits BufferUnits { get; }
    }
}
