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

namespace gView.DataSources.Shape
{
    [RegisterPlugIn("BC0F0E24-075D-437c-ACBF-48BA74906009")]
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

        public Task<int> CreateDataset(string name, ISpatialReference sRef)
        {
            return Task.FromResult<int>(Create(name) ? 0 : -1);
        }

        public Task<int> CreateFeatureClass(string dsname, string fcname, IGeometryDef geomDef, IFieldCollection fields)
        {
            if (geomDef == null || fields == null)
            {
                return Task.FromResult(-1);
            }

            string filename = _directoryName + @"/" + fcname;
            FieldCollection f = new FieldCollection();

            foreach (IField field in fields.ToEnumerable())
            {
                f.Add(field);
            }

            if (!SHPFile.Create(filename, geomDef, f))
            {
                return Task.FromResult(-1);
            }

            return Task.FromResult(0);
        }

        async public Task<IFeatureDataset> GetDataset(string name)
        {
            if (name.ToLower() == _directoryName.ToLower() ||
                name.ToLower() == "esri shapefile")
            {
                ShapeDataset dataset = new ShapeDataset();
                await dataset.SetConnectionString(_directoryName);
                await dataset.Open();

                return dataset;
            }
            else
            {
                ShapeDataset dataset = new ShapeDataset();
                await dataset.SetConnectionString(name);
                await dataset.Open();

                return dataset;
            }
        }

        public Task<string[]> DatasetNames()
        {
            throw new Exception("The method or operation is not implemented.");
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

        public Task<bool> DeleteFeatureClass(string fcName)
        {
            if (_name == "")
            {
                return Task.FromResult(false);
            }

            SHPFile file = new SHPFile(_name + @"/" + fcName + ".shp");
            return Task.FromResult(file.Delete());
        }

        public Task<bool> RenameDataset(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public Task<bool> RenameFeatureClass(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        async public Task<IFeatureCursor> Query(IFeatureClass fc, IQueryFilter filter)
        {
            return await fc.GetFeatures(filter);
        }

        #endregion

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

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region IFeatureUpdater Member

        public Task<bool> Insert(IFeatureClass fClass, IFeature feature)
        {
            if (fClass == null || feature == null)
            {
                return Task.FromResult<bool>(false);
            }

            List<IFeature> features = new List<IFeature>();
            features.Add(feature);
            return Insert(fClass, features);
        }

        public Task<bool> Insert(IFeatureClass fClass, List<IFeature> features)
        {
            if (fClass == null || !(fClass.Dataset is ShapeDataset) || features == null)
            {
                return Task.FromResult<bool>(false);
            }

            if (features.Count == 0)
            {
                return Task.FromResult<bool>(true);
            }

            SHPFile shpFile = new SHPFile(fClass.Dataset.ConnectionString + @"/" + fClass.Name + ".shp");

            foreach (IFeature feature in features)
            {
                if (!shpFile.WriteShape(feature))
                {
                    return Task.FromResult<bool>(false);
                }
            }

            return Task.FromResult<bool>(true);
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
