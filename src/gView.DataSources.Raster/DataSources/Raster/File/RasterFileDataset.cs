using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.Data.Metadata;
using gView.Framework.Geometry;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataSources.Raster.File
{
    [RegisterPlugIn("D4812641-3F53-48eb-A66C-FC0203980C79")]
    public class RasterFileDataset : DatasetMetadata, IRasterFileDataset
    {
        private List<IDatasetElement> _layers = new List<IDatasetElement>();
        private string _directory = "";
        private DatasetState _state = DatasetState.opened;

        public RasterFileDataset() { }

        #region IDataset Members

        public void Dispose()
        {

        }

        public string ConnectionString
        {
            get
            {
                return _directory;
            }
        }
        public Task<bool> SetConnectionString(string value)
        {
            _directory = value;

            return Task.FromResult(true);
        }

        public string DatasetGroupName
        {
            get { return "Raster"; }
        }

        public string DatasetName
        {
            get { return "Raster File"; }
        }

        public string ProviderName
        {
            get { return "Raster File"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public Task<bool> Open()
        {
            return Task.FromResult(false);
        }

        public string LastErrorMessage
        {
            get { return ""; }
            set { }
        }

        public int order
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        public IDatasetEnum DatasetEnum
        {
            get { return null; }
        }

        public Task<List<IDatasetElement>> Elements()
        {
            List<IDatasetElement> ret = new List<IDatasetElement>();
            foreach (IRasterLayer layer in _layers)
            {
                ret.Add(layer);
            }

            return Task.FromResult(ret);
        }

        public string Query_FieldPrefix
        {
            get { return ""; }
        }

        public string Query_FieldPostfix
        {
            get { return ""; }
        }

        public IDatabase Database
        {
            get { return null; }
        }

        public Task<IDatasetElement> Element(string title)
        {
            foreach (IDatasetElement element in _layers)
            {
                if (element == null)
                {
                    continue;
                }

                if (element.Title == title)
                {
                    return Task.FromResult(element);
                }
            }

            try
            {
                if (_directory != "")
                {
                    FileInfo fi = new FileInfo(_directory + @"/" + title);
                    if (fi.Exists)
                    {
                        return Task.FromResult<IDatasetElement>(AddRasterFile(fi.FullName));
                    }
                }
            }
            catch { }

            return Task.FromResult<IDatasetElement>(null);
        }

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }
        #endregion

        #region IRasterFileDataset Member

        public IRasterLayer AddRasterFile(string filename)
        {
            return AddRasterFile(filename, null);
        }
        public IRasterLayer AddRasterFile(string filename, IPolygon polygon)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (_directory == "")
                {
                    _directory = fi.Directory.FullName;
                }
                else if (_directory.ToLower() != fi.Directory.FullName.ToLower())
                {
                    return null;
                }
                if (fi.Extension.ToLower() == ".sid" || fi.Extension.ToLower() == ".jp2")
                {
                    MrSidFileClass rasterClass = (polygon == null) ?
                        new MrSidFileClass(this, filename) :
                        new MrSidFileClass(this, filename, polygon);

                    RasterLayer layer = new RasterLayer(rasterClass);
                    if (rasterClass.isValid)
                    {
                        _layers.Add(layer);
                    }
                    return layer;
                }
                else
                {
                    RasterFileClass rasterClass = (polygon == null) ? new RasterFileClass(this, filename) : new RasterFileClass(this, filename, polygon);
                    RasterLayer layer = new RasterLayer(rasterClass);
                    if (rasterClass.isValid)
                    {
                        _layers.Add(layer);
                    }
                    return layer;
                }
            }
            catch { }
            return null;
        }

        public string SupportedFileFilter
        {
            get
            {
                if (PlugInManager.Create(new Guid("43DFABF1-3D19-438c-84DA-F8BA0B266592")) is IRasterFileDataset)
                {
                    // GDAL is installed!!
                    return "*.sid|*.jp2";
                }
                return "*.sid|*.jp2|*.tif|*.tiff|*.png|*.jpg|*.jpeg";
            }
        }

        public int SupportsFormat(string extension)
        {
            switch (extension.ToLower())
            {
                case ".sid":
                case ".jp2":
                case ".tif":
                case ".tiff":
                case ".png":
                case ".jpg":
                case ".jpeg":
                    return 100;
            }
            return -1;
        }

        #endregion

        #region IRasterDataset Members

        public Task<IEnvelope> Envelope()
        {
            Envelope env = null;

            foreach (IRasterLayer layer in _layers)
            {
                if (layer.RasterClass == null || layer.RasterClass.Polygon == null)
                {
                    continue;
                }

                if (env == null)
                {
                    env = new Envelope(layer.RasterClass.Polygon.Envelope);
                }
                else
                {
                    env.Union(layer.RasterClass.Polygon.Envelope);
                }
            }
            return Task.FromResult((IEnvelope)env);
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult<ISpatialReference>(null);
        }
        public void SetSpatialReference(ISpatialReference sRef)
        {

        }

        #endregion

        #region IPersistableLoadAsync Members

        public string PersistID
        {
            get { return ""; }
        }

        public Task<bool> LoadAsync(IPersistStream stream)
        {
            _directory = (string)stream.Load("Directory");
            if (_directory == null)
            {
                _directory = "";
            }

            return Task.FromResult(true);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Directory", _directory);
        }

        #endregion
    }
}
