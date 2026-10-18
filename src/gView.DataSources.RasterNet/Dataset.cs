using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Data;
using gView.Framework.Data.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataSources.RasterNet
{
    /// <summary>
    /// Raster files read with RasterNet.Core, a managed and thread-safe raster library: GeoTIFF/COG
    /// (lossless compressions and JPEG) and Arc/Info Binary Grids. It takes precedence over the GDAL dataset
    /// for these formats; files RasterNet cannot read are not added, so the next provider (GDAL) is used.
    /// </summary>
    [RegisterPlugIn("5D52DA35-EA83-4B52-ADD3-4EEF0FAB12AA")]
    public class Dataset : DatasetMetadata, IRasterFileDataset
    {
        // Above the GDAL dataset (200) and the raster file dataset (100).
        private const int Priority = 250;

        private readonly List<IDatasetElement> _layers = new List<IDatasetElement>();
        private string _connectionString = "";

        public IRasterLayer AddRasterFile(string filename)
        {
            return AddRasterFile(filename, null);
        }

        public IRasterLayer AddRasterFile(string filename, IPolygon polygon)
        {
            try
            {
                var layer = new RasterLayer(new RasterClass(this, filename, polygon));
                _layers.Add(layer);
                return layer;
            }
            catch (Exception ex)
            {
                // Not supported or not readable: leave the file to the next provider.
                LastErrorMessage = $"{Path.GetFileName(filename)}: {ex.Message}";
                return null;
            }
        }

        #region IRasterFileDataset

        public string SupportedFileFilter => "*.tif|*.tiff|*.jp2|*.j2k|*.jpg|*.jpeg|*.png|*.gif|*.bmp|*.webp|w001001.adf";

        public int SupportsFormat(string extension)
        {
            switch (extension?.ToLowerInvariant())
            {
                case ".tif":
                case ".tiff":
                case ".jp2":
                case ".j2k":
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                case ".webp":
                case ".adf":
                    return Priority;
                default:
                    return -1;
            }
        }

        #endregion

        #region IRasterDataset

        public Task<IEnvelope> Envelope()
        {
            IEnvelope envelope = null;
            foreach (var element in _layers)
            {
                if (element.Class is IRasterClass rasterClass && rasterClass.Polygon != null)
                {
                    if (envelope == null)
                    {
                        envelope = rasterClass.Polygon.Envelope;
                    }
                    else
                    {
                        envelope.Union(rasterClass.Polygon.Envelope);
                    }
                }
            }
            return Task.FromResult(envelope);
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult<ISpatialReference>(null);
        }

        public void SetSpatialReference(ISpatialReference sRef)
        {
        }

        #endregion

        #region IDataset

        public string ConnectionString
        {
            get
            {
                var filenames = new List<string>();
                foreach (var element in _layers)
                {
                    if (element.Class is IRasterFile rasterFile)
                    {
                        filenames.Add(rasterFile.Filename);
                    }
                }
                _connectionString = string.Join(";", filenames);
                return _connectionString;
            }
        }

        public Task<bool> SetConnectionString(string value)
        {
            foreach (var element in _layers)
            {
                (element.Class as IDisposable)?.Dispose();
            }
            _layers.Clear();

            _connectionString = value ?? "";
            foreach (var filename in _connectionString.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    AddRasterFile(filename.Trim());
                }
            }
            return Task.FromResult(true);
        }

        public string DatasetGroupName => "RasterNet Image Dataset";

        public string DatasetName => "RasterNet";

        public string ProviderName => "gView";

        public DatasetState State => DatasetState.opened;

        public Task<bool> Open()
        {
            return Task.FromResult(true);
        }

        public string LastErrorMessage { get; set; } = "";

        public Task<List<IDatasetElement>> Elements()
        {
            return Task.FromResult(new List<IDatasetElement>(_layers));
        }

        public string Query_FieldPrefix => throw new NotImplementedException();

        public string Query_FieldPostfix => throw new NotImplementedException();

        public IDatabase Database => null;

        public Task<IDatasetElement> Element(string title)
        {
            return Task.FromResult(_layers.Find(element => element.Title == title));
        }

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region IPersistable

        public async Task<bool> LoadAsync(IPersistStream stream)
        {
            await SetConnectionString((string)stream.Load("filename", string.Empty));
            return true;
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("filename", ConnectionString);
        }

        #endregion

        public void Dispose()
        {
            foreach (var element in _layers)
            {
                (element.Class as IDisposable)?.Dispose();
            }
            _layers.Clear();
        }
    }
}
