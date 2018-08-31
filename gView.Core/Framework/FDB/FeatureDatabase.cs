using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.Network;

namespace gView.Framework.FDB
{
    public interface IDatabase : IErrorMessage ,IDisposable
    {
        bool Create(string name);
        bool Open(string name);

        string lastErrorMsg { get; }
        Exception lastException { get; }
    }

    public interface IDatabaseNames
    {
        string TableName(string tableName);
        string DbColName(string fieldName);
    }
	/// <summary>
	/// 
	/// </summary>
	public interface IFeatureDatabase : IDatabase, IFeatureUpdater 
	{
		//int OpenDataset(string name);
        //int OpenFeatureClass(int DatasetID,string name);

        int CreateDataset(string name, ISpatialReference sRef);
		int CreateFeatureClass(
			string dsname,
			string fcname,
			IGeometryDef geomDef,
			IFields Fields);

        IFeatureDataset this[string name] { get; }

        bool DeleteDataset(string dsName);
        bool DeleteFeatureClass(string fcName);

        bool RenameDataset(string name, string newName);
        bool RenameFeatureClass(string name, string newName);

		IFeatureCursor Query(IFeatureClass fc,IQueryFilter filter);

        string[] DatasetNames { get; }
	}
    public interface IFeatureDatabase2 : IFeatureDatabase
    {
        int CreateDataset(string name, ISpatialReference sRef, ISpatialIndexDef sIndexDef);
    }
    public interface IFeatureDatabase3 : IFeatureDatabase2
    {
        int DatasetID(string dsname);
        int FeatureClassID(int datasetId, string fcname);
        int GetFeatureClassID(string fcname);
    }

    public interface IFileFeatureDatabase : IFeatureDatabase, IFeatureUpdater
    {
        bool Flush(IFeatureClass fc);

        string DatabaseName { get; }
        int MaxFieldNameLength { get; }
    }

    public interface IImageDB
    {
        int CreateImageDataset(string name, ISpatialReference sRef, ISpatialIndexDef sIndexDef, string imageSpace, IFields fields);
        bool IsImageDataset(string dsname, out string imageSpace);
    }

	public interface IReportProgress 
	{
		void AddDelegate(object Delegate);
	}

	public interface IFeatureUpdater : IErrorMessage
	{
		bool Insert(IFeatureClass fClass,IFeature feature);
        bool Insert(IFeatureClass fClass, List<IFeature> features);

        bool Update(IFeatureClass fClass, IFeature feature);
        bool Update(IFeatureClass fClass, List<IFeature> features);

        bool Delete(IFeatureClass fClass, int oid);
        bool Delete(IFeatureClass fClass, string where);

        int SuggestedInsertFeatureCountPerTransaction { get; }
	}

    public interface IFeatureImportEvents
    {
        void BeforeInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc);
        void AfterInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc);
    
    }

    public interface IAltertable
    {
        bool AlterTable(string table, IField oldField, IField newField);
    }
    public interface IAlterDatabase
    {
        bool DropTable(string tabname);
    }
	public interface ISpatialIndexNode 
	{
		int NID { get ; }
		int PID { get ; }
		short Page { get ; }
		IGeometry Rectangle { get; }
		List<int> IDs { get; }
	}

	public interface ISpatialIndexNode2 
	{
		int NID { get ; }
		int PID { get ; }
		short Page { get ; }
	}

	public interface ISpatialTreeInfo 
	{
		string type { get; }
		IEnvelope Bounds { get; }
		double SpatialRatio { get; }
		int MaxFeaturesPerNode { get; }

		List<SpatialIndexNode> SpatialIndexNodes { get ; }
	}

	public class SpatialIndexNode : ISpatialIndexNode,IComparable
	{
		private int _NID=0,_PID=0;
		private short _page;
		private IGeometry _geom;
		private List<int> _IDs;

		#region ISpatialIndexNode Member

		public int NID
		{
			get
			{
				return _NID;
			}
			set 
			{
				_NID=value;
			}
		}

		public int PID
		{
			get
			{
				return _PID;
			}
			set 
			{
				_PID=value;
			}
		}

		public IGeometry Rectangle
		{
			get
			{
				return _geom;
			}
			set 
			{
				_geom=value;		
			}
		}
		public List<int> IDs
		{
			get
			{
				return _IDs;
			}
			set 
			{
				_IDs=value;
			}
		}

		public short Page
		{
			get
			{
				return _page;
			}
			set 
			{
				_page=value;
			}
		}
		#endregion

		#region IComparable Member

		public int CompareTo(object obj)
		{
			return (this.NID < (((ISpatialIndexNode)obj).NID)) ? -1 : 1;
			//return 0;
		}

		#endregion
	}

	public class SpatialIndexNode2 : ISpatialIndexNode2
	{
		private int _NID=0,_PID=0;
		private short _page;

		#region ISpatialIndexNode Member

		public int NID
		{
			get
			{
				return _NID;
			}
			set 
			{
				_NID=value;
			}
		}

		public int PID
		{
			get
			{
				return _PID;
			}
			set 
			{
				_PID=value;
			}
		}

		public short Page
		{
			get
			{
				return _page;
			}
			set 
			{
				_page=value;
			}
		}

		#endregion

	}

    public interface ISpatialIndexDef
    {
        GeometryFieldType GeometryType { get; }
        IEnvelope SpatialIndexBounds { get; }
        double SplitRatio { get; }
        int MaxPerNode { get; }
        int Levels { get; }
        ISpatialReference SpatialReference { get; }
        bool ProjectTo(ISpatialReference sRef);
    }

    public interface IFDBDataset
    {
        ISpatialIndexDef SpatialIndexDef { get; }
    }

	public interface ISpatialIndex 
	{
		bool InitIndex(string FCName);
		bool InsertNodes(string FCName,List<SpatialIndexNode> nodes);

		DataTable QueryIDs(IEnvelope env);
	}

	public interface IIndexTree 
	{
		List<int> FindShapeIds( IEnvelope Bounds );
	}

    public interface IAutoField : IField
    {
        string AutoFieldName { get; }
        string AutoFieldDescription { get; }

        string AutoFieldPrimayName { get; }
        FieldType AutoFieldType { get; }

        bool OnInsert(IFeatureClass fc, IFeature feature);
        bool OnUpdate(IFeatureClass fc, IFeature feature);
    }

    public interface IFDBDatabase : IFeatureDatabase3
    {
        bool SetSpatialIndexBounds(string FCName, string TreeType, IEnvelope Bounds, double SpatialRatio, int maxPerNode, int maxLevels);
        ISpatialIndexDef SpatialIndexDef(string dsName);
        bool ShrinkSpatialIndex(string fcName, List<long> NIDs);
        bool SetFeatureclassExtent(string fcName, IEnvelope envelope);
        bool CalculateExtent(string fcName);
    }
}
