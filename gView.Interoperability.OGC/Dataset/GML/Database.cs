using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.FDB;
using System.IO;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Interoperability.OGC.Dataset.GML
{
    [gView.Framework.system.RegisterPlugIn("93474BC6-9E24-45ff-B21D-1D8CF0E9D30A")]
    public class Database : IFileFeatureDatabase
    {
        private string _errMsg = String.Empty, _name = String.Empty;
        private string _directoryName = String.Empty;
        private GmlVersion _gmlVersion = GmlVersion.v1;

        internal string DirectoryName
        {
            set
            {
                try
                {
                    _directoryName = value;
                    FileInfo fi = new FileInfo(_directoryName);
                    if (fi.Exists)
                        _directoryName = fi.DirectoryName;
                }
                catch { }
            }
            get { return _directoryName; }
        }

        #region IDatabase Member

        public bool Create(string name)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(name);
                if (!di.Exists)
                    di.Create();

                _directoryName = name;
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
            catch (Exception ex)
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

        #region IFeatureDatabase Member

        public int CreateDataset(string name, gView.Framework.Geometry.ISpatialReference sRef)
        {
            return Create(name) ? 0 : -1;
        }

        public int CreateFeatureClass(string dsname, string fcname, gView.Framework.Geometry.IGeometryDef geomDef, IFields fields)
        {
            if (geomDef == null || fields == null) return -1;

            string filename = _directoryName + @"\" + fcname + ".gml";
            Fields f = new Fields();

            foreach (IField field in fields.ToEnumerable())
                f.Add(field);

            if (!GMLFile.Create(filename, geomDef, f, _gmlVersion))
                return -1;

            return 0;
        }

        public gView.Framework.Data.IFeatureDataset this[string name]
        {
            get
            {
                if (name.ToLower() == "gml dataset" ||
                    name.ToLower() == _directoryName.ToLower())
                {
                    Dataset dataset = new Dataset();
                    dataset.ConnectionString = _directoryName;
                    dataset.Open();

                    return dataset;
                }
                else
                {
                    Dataset dataset = new Dataset();
                    dataset.ConnectionString = name;
                    dataset.Open();

                    return dataset;
                }
                return null;
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

        public bool RenameDataset(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public bool RenameFeatureClass(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string[] DatasetNames
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public bool DeleteFeatureClass(string fcName)
        {
            string filename = _directoryName + @"\" + fcName + ".gml";

            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {
                GMLFile file = new GMLFile(fi.FullName);
                return file.Delete();
            }
            return false;
        }

        public gView.Framework.Data.IFeatureCursor Query(gView.Framework.Data.IFeatureClass fc, gView.Framework.Data.IQueryFilter filter)
        {
            if (fc == null) return null;

            return fc.GetFeatures(filter);
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            
        }

        #endregion

        #region IFeatureUpdater Member
        private Dictionary<string, GMLFile> _gmlFiles = new Dictionary<string, GMLFile>();

        public bool Insert(gView.Framework.Data.IFeatureClass fClass, gView.Framework.Data.IFeature feature)
        {
            if (fClass == null || feature == null) return false;

            List<IFeature> features = new List<IFeature>();
            return Insert(fClass, features);
        }

        public bool Insert(gView.Framework.Data.IFeatureClass fClass, List<gView.Framework.Data.IFeature> features)
        {
            if (fClass == null || !(fClass.Dataset is Dataset) || features == null) return false;

            GMLFile gmlFile=null;
            try
            {
                // use always the same GMLFile...
                if (_gmlFiles.TryGetValue(((Dataset)fClass.Dataset).ConnectionString, out gmlFile))
                {
                }
                else
                {
                    gmlFile = ((Dataset)fClass.Dataset).GMLFile;
                    _gmlFiles.Add(((Dataset)fClass.Dataset).ConnectionString, gmlFile);
                }
            }
            catch
            {
                return false;
            }
            if (gmlFile == null) return false;

            foreach (IFeature feature in features)
            {
                if (feature == null) continue;

                if (!gmlFile.AppendFeature(fClass, feature)) return false;
            }

            return true;
        }

        public bool Update(gView.Framework.Data.IFeatureClass fClass, gView.Framework.Data.IFeature feature)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Update(gView.Framework.Data.IFeatureClass fClass, List<gView.Framework.Data.IFeature> features)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Delete(gView.Framework.Data.IFeatureClass fClass, int oid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Delete(gView.Framework.Data.IFeatureClass fClass, string where)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int SuggestedInsertFeatureCountPerTransaction
        {
            get { return 50; }
        }

        #endregion

        #region IFileFeatureDatabase Member

        public bool Flush(IFeatureClass fClass)
        {
            if (fClass == null || !(fClass.Dataset is Dataset)) return false;

            GMLFile gmlFile = null;
            try
            {
                // use always the same GMLFile...
                if (!_gmlFiles.TryGetValue(((Dataset)fClass.Dataset).ConnectionString, out gmlFile))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            if (gmlFile == null) return false;

            return gmlFile.Flush();
        }

        public string DatabaseName
        {
            get { return "OGC GML"; }
        }

        public int MaxFieldNameLength
        {
            get { return 255; }
        }

        #endregion


        
    }
}
