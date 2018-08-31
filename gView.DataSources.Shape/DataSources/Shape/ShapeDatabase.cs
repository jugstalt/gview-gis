using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.Framework.FDB;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.DataSources.Shape
{
    [gView.Framework.system.RegisterPlugIn("BC0F0E24-075D-437c-ACBF-48BA74906009")]
    public class ShapeDatabase : IFileFeatureDatabase
    {
        private string _errMsg = "";
        private string _name = "";
        private string _directoryName = String.Empty;
        
        internal string DirectoryName
        {
            set { _directoryName = value; }
        }

        #region IFeatureDatabase Member

        public int CreateDataset(string name, ISpatialReference sRef)
        {
            return Create(name) ? 0 : -1;
        }

        public int CreateFeatureClass(string dsname, string fcname, IGeometryDef geomDef, IFields fields)
        {
            if (geomDef == null || fields == null) return -1;

            string filename = _directoryName + @"\" + fcname;
            Fields f = new Fields();

            foreach (IField field in fields.ToEnumerable())
                f.Add(field);

            if (!SHPFile.Create(filename, geomDef, f))
                return -1;

            return 0;
        }

        public IFeatureDataset this[string name]
        {
            get
            {
                if (name.ToLower() == _directoryName.ToLower() ||
                    name.ToLower() == "esri shapefile")
                {
                    ShapeDataset dataset = new ShapeDataset();
                    dataset.ConnectionString = _directoryName;
                    dataset.Open();

                    return dataset;
                }
                else
                {
                    ShapeDataset dataset = new ShapeDataset();
                    dataset.ConnectionString = name;
                    dataset.Open();

                    return dataset;
                }

                return null;
            }
        }

        public string[] DatasetNames
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public bool DeleteDataset(string dsName)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(dsName);
                if (!di.Exists) di.Delete();

                return true;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public bool DeleteFeatureClass(string fcName)
        {
            if(_name=="") return false;
            SHPFile file = new SHPFile(_name + @"\" + fcName + ".shp");
            return file.Delete();
        }

        public bool RenameDataset(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public bool RenameFeatureClass(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IFeatureCursor Query(IFeatureClass fc, IQueryFilter filter)
        {
            return fc.GetFeatures(filter);
        }

        #endregion

        #region IDatabase Member

        public bool Create(string name)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(name);
                if (!di.Exists) di.Create();

                return true;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public bool Open(string name)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(name);
                if (di.Exists)
                {
                    _name = _directoryName = name;
                    return true;
                }
                else
                {
                    _name = name;
                    _errMsg = "Directory not exists!";
                    return false;
                }
            }
            catch(Exception ex)
            {
                _errMsg = ex.Message;
                _name = "";
                return false;
            }
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public Exception lastException { get { return null; } }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            
        }

        #endregion

        #region IFeatureUpdater Member

        public bool Insert(IFeatureClass fClass, IFeature feature)
        {
            if (fClass == null || feature == null) return false;

            List<IFeature> features = new List<IFeature>();
            features.Add(feature);
            return Insert(fClass, features);
        }

        public bool Insert(IFeatureClass fClass, List<IFeature> features)
        {
            if (fClass == null || !(fClass.Dataset is ShapeDataset) || features == null) return false;
            if (features.Count == 0) return true;

            SHPFile shpFile = new SHPFile(fClass.Dataset.ConnectionString + @"\" + fClass.Name + ".shp");

            foreach (IFeature feature in features)
            {
                if (!shpFile.WriteShape(feature))
                    return false;
            }

            return true;
        }

        public bool Update(IFeatureClass fClass, IFeature feature)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Update(IFeatureClass fClass, List<IFeature> features)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Delete(IFeatureClass fClass, int oid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Delete(IFeatureClass fClass, string where)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int SuggestedInsertFeatureCountPerTransaction
        {
            get { return 50; }
        }

        #endregion

        #region IFileFeatureDatabase Member

        public bool Flush(IFeatureClass fc)
        {
            return true;
        }

        public string DatabaseName
        {
            get { return "ESRI Shape file"; }
        }

        public int MaxFieldNameLength
        {
            get { return 10; }
        }

        #endregion


        
    }
}
