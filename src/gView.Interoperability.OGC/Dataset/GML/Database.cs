using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.system;
using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.Interoperability.OGC.Dataset.GML
{
    [RegisterPlugIn("93474BC6-9E24-45ff-B21D-1D8CF0E9D30A")]
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
                    {
                        _directoryName = fi.DirectoryName;
                    }
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
                {
                    di.Create();
                }

                _directoryName = name;
                return true;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public Task<bool> Open(string name)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(name);
                if (di.Exists)
                {
                    _name = _directoryName = name;
                    return Task.FromResult(true);
                }
                else
                {
                    _name = name;
                    _errMsg = "Directory not exists!";
                    return Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                _name = "";
                return Task.FromResult(false);
            }
        }

        public string LastErrorMessage
        {
            get { return _errMsg; }
            set { _errMsg = value; }
        }

        public Exception LastException { get { return null; } }
        #endregion

        #region IFeatureDatabase Member

        public Task<int> CreateDataset(string name, ISpatialReference sRef)
        {
            return Task.FromResult<int>(Create(name) ? 0 : -1);
        }

        async public Task<int> CreateFeatureClass(string dsname, string fcname, IGeometryDef geomDef, IFieldCollection fields)
        {
            if (geomDef == null || fields == null)
            {
                return -1;
            }

            string filename = _directoryName + @"/" + fcname + ".gml";
            FieldCollection f = new FieldCollection();

            foreach (IField field in fields.ToEnumerable())
            {
                f.Add(field);
            }

            if (!await GMLFile.Create(filename, geomDef, f, _gmlVersion))
            {
                return -1;
            }

            return 0;
        }

        async public Task<IFeatureDataset> GetDataset(string name)
        {
            if (name.ToLower() == "gml dataset" ||
                name.ToLower() == _directoryName.ToLower())
            {
                Dataset dataset = new Dataset();
                await dataset.SetConnectionString(_directoryName);
                await dataset.Open();

                return dataset;
            }
            else
            {
                Dataset dataset = new Dataset();
                await dataset.SetConnectionString(name);
                await dataset.Open();

                return dataset;
            }
        }

        public Task<bool> DeleteDataset(string dsName)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(dsName);
                if (!di.Exists)
                {
                    di.Delete();
                }

                return Task.FromResult<bool>(true);
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return Task.FromResult<bool>(false);
            }
        }

        public Task<bool> RenameDataset(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public Task<bool> RenameFeatureClass(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Task<string[]> DatasetNames()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        async public Task<bool> DeleteFeatureClass(string fcName)
        {
            string filename = _directoryName + @"/" + fcName + ".gml";

            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {
                GMLFile file = await GMLFile.Create(fi.FullName);
                return file.Delete();
            }
            return false;
        }

        async public Task<IFeatureCursor> Query(IFeatureClass fc, IQueryFilter filter)
        {
            if (fc == null)
            {
                return null;
            }

            return await fc.GetFeatures(filter);
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region IFeatureUpdater Member
        private Dictionary<string, GMLFile> _gmlFiles = new Dictionary<string, GMLFile>();

        public Task<bool> Insert(IFeatureClass fClass, IFeature feature)
        {
            if (fClass == null || feature == null)
            {
                return Task.FromResult<bool>(false);
            }

            List<IFeature> features = new List<IFeature>();
            return Insert(fClass, features);
        }

        async public Task<bool> Insert(IFeatureClass fClass, List<IFeature> features)
        {
            if (fClass == null || !(fClass.Dataset is Dataset) || features == null)
            {
                return false;
            }

            GMLFile gmlFile = null;
            try
            {
                // use always the same GMLFile...
                if (_gmlFiles.TryGetValue(((Dataset)fClass.Dataset).ConnectionString, out gmlFile))
                {
                }
                else
                {
                    gmlFile = await ((Dataset)fClass.Dataset).GetGMLFile();
                    _gmlFiles.Add(((Dataset)fClass.Dataset).ConnectionString, gmlFile);
                }
            }
            catch
            {
                return false;
            }
            if (gmlFile == null)
            {
                return false;
            }

            foreach (IFeature feature in features)
            {
                if (feature == null)
                {
                    continue;
                }

                if (!gmlFile.AppendFeature(fClass, feature))
                {
                    return false;
                }
            }

            return true;
        }

        public Task<bool> Update(IFeatureClass fClass, IFeature feature)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Task<bool> Update(IFeatureClass fClass, List<IFeature> features)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Task<bool> Delete(IFeatureClass fClass, int oid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Task<bool> Delete(IFeatureClass fClass, string where)
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
            if (fClass == null || !(fClass.Dataset is Dataset))
            {
                return false;
            }

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
            if (gmlFile == null)
            {
                return false;
            }

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
