using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Collections.Generic;

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

        IEnumerable<string> QuerySubFields { get; }
        string SubFields { get ; set ; }
        string SubFieldsAndAlias { get; }
		string WhereClause { set ; get ; }
        string JsonWhereClause { get; }
		string OrderBy { set; get; }
        bool NoLock { set; get; }
		int BeginRecord { get ; set ;}
        int Limit { get; set; }
		int LastQueryFeatureCount { get ; }
		bool HasMore { get ; }

		string fieldPrefix { get ; set ; }
		string fieldPostfix { get ; set ; }

        ISpatialReference FeatureSpatialReference { get; set; }
        ISpatialReference ContextLayerDefaultSpatialReference { get; set; }

        ICancelTracker CancelTracker { get; set; }

        bool IgnoreUndefinedFields { get; set; }

        double MapScale { get; set; }

        IDatasetCachingContext DatasetCachingContext { get; set; }
    }
}
