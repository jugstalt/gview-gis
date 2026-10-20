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
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.RasterNet
{
    /// <summary>
    /// Raster files read with RasterNet.Core, a managed and thread-safe raster library: GeoTIFF/COG
    /// (lossless compressions and JPEG) and Arc/Info Binary Grids, and GeoTIFF/COG given by an http(s) URL. It takes precedence over the GDAL dataset
    /// for these formats; files RasterNet cannot read are not added, so the next provider (GDAL) is used.
    /// It is also the fallback for the GDAL and the MrSID raster file datasets, if their native libraries
    /// are not installed: it loads what they persisted (GDAL: a file list, MrSID: a directory).
    /// </summary>
    [RegisterPlugIn("5D52DA35-EA83-4B52-ADD3-4EEF0FAB12AA")]
    [PlugInFallbackFor("43DFABF1-3D19-438c-84DA-F8BA0B266592")]  // GDAL raster file dataset
    [PlugInFallbackFor("D4812641-3F53-48eb-A66C-FC0203980C79")]  // MrSID raster file dataset
    public class Dataset : DatasetMetadata, IRasterFileDataset
    {
        // Above the GDAL dataset (200) and the raster file dataset (100).
        private const int Priority = 250;

        private readonly List<IDatasetElement> _layers = new List<IDatasetElement>();
        private string _connectionString = "";

        // Directory mode: only for datasets persisted by the MrSID raster file dataset (fallback).
        // Elements are added lazily by their title (the file name) within this directory.
        private string _directory = null;

        // Files of the connection string that could not be opened (e.g. persisted by the GDAL dataset
        // in a format RasterNet can't read). They are kept in the connection string, so saving a
        // project does not drop them, and their error is reported by Element(title) for that file only.
        private readonly List<UnreadableFile> _unreadableFiles = new List<UnreadableFile>();

        private record UnreadableFile(string Filename, string Title, string ErrorMessage);

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
                LastErrorMessage = $"{TitleOf(filename)}: {ex.Message}";
                return null;
            }
        }

        /// <summary>The layer title of a file: its name, for a URL without the query (it may hold credentials).</summary>
        private static string TitleOf(string filename)
            => RasterNetSettings.IsUrl(filename, out var uri) ? RasterClass.TitleOf(uri) : Path.GetFileName(filename);

        #region IRasterFileDataset

        public string SupportedFileFilter => "*.tif|*.tiff|*.jp2|*.j2k|*.jpg|*.jpeg|*.png|*.gif|*.bmp|*.webp|*.bil|*.bip|*.bsq|*.flt|*.asc|*.xyz|*.hgt|w001001.adf";

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
                case ".bil":
                case ".bip":
                case ".bsq":
                case ".flt":
                case ".asc":
                case ".xyz":
                case ".hgt":
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
                if (_directory != null)
                {
                    return _directory;
                }

                var filenames = new List<string>();
                foreach (var element in _layers)
                {
                    if (element.Class is IRasterFile rasterFile)
                    {
                        filenames.Add(rasterFile.Filename);
                    }
                }
                filenames.AddRange(_unreadableFiles.Select(file => file.Filename));
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
            _directory = null;
            _unreadableFiles.Clear();

            _connectionString = value ?? "";
            foreach (var filename in _connectionString.Split(';').Select(f => f.Trim()))
            {
                if (!string.IsNullOrEmpty(filename) && AddRasterFile(filename) == null)
                {
                    _unreadableFiles.Add(new UnreadableFile(filename, TitleOf(filename), LastErrorMessage));
                }
            }

            // A single unreadable file is not an error of the whole dataset (that would drop all its
            // layers when a map is loaded); it is reported by Element(title) for that file.
            LastErrorMessage = "";
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
            LastErrorMessage = "";

            var element = _layers.Find(element => element.Title == title);

            if (element == null && _unreadableFiles.Find(file => file.Title == title) is UnreadableFile unreadable)
            {
                LastErrorMessage = unreadable.ErrorMessage;
            }
            else if (element == null && _directory != null && !string.IsNullOrEmpty(title))
            {
                string filename = Path.Combine(_directory, title);
                if (Path.GetExtension(filename).Equals(".sid", StringComparison.OrdinalIgnoreCase))
                {
                    LastErrorMessage = $"{title}: MrSID files (.sid) can't be read without the MrSID native libraries";
                }
                else if (File.Exists(filename))
                {
                    element = AddRasterFile(filename);
                }
            }

            return Task.FromResult(element);
        }

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region IPersistable

        public async Task<bool> LoadAsync(IPersistStream stream)
        {
            if (stream.Load("Directory", null) is string directory)
            {
                // persisted by the MrSID raster file dataset
                await SetConnectionString(string.Empty);
                _directory = directory;
                return true;
            }

            await SetConnectionString((string)stream.Load("filename", string.Empty));
            return true;
        }

        public void Save(IPersistStream stream)
        {
            if (_directory != null)
            {
                stream.Save("Directory", _directory);
                return;
            }

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
