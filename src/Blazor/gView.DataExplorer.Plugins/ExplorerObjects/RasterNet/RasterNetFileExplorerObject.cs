using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Extensions;
using gView.DataSources.RasterNet;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.DataExplorer.Abstraction;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.RasterNet;

/// <summary>
/// A raster file read with RasterNet.Core (GeoTIFF/COG, Arc/Info Binary Grid). Files that GDAL can read as well
/// also show up as GDAL raster file, so both readers can be compared side by side.
/// </summary>
[RegisterPlugIn("874D905B-B41D-4D35-9A63-680195652ABF")]
public class RasterNetFileExplorerObject :
            ExplorerObjectCls<IExplorerObject, RasterClass>,
            IExplorerFileObject,
            IExplorerObjectCustomContentValues
{
    private string _filename = "";
    private IRasterClass? _class = null;

    public RasterNetFileExplorerObject() : base() { }

    private RasterNetFileExplorerObject(IExplorerObject parent, string filename)
        : base(parent, 2)
    {
        _filename = filename;
    }

    #region IExplorerFileObject Members

    public string Filter => "*.tif|*.tiff|*.jp2|*.j2k|*.jpg|*.jpeg|*.png|*.gif|*.bmp|*.webp|*.bil|*.bip|*.bsq|*.flt|*.asc|*.xyz|*.hgt|w001001.adf";

    public string Icon => "webgis:georef-image";

    #endregion

    #region IExplorerObjectCustomContentValues

    public IDictionary<string, object?> GetCustomContentValues()
        => _filename.GetFileProperties();

    #endregion

    #region IExplorerObject Members

    public string Name
    {
        get
        {
            try
            {
                return new FileInfo(_filename).Name;
            }
            catch
            {
                return "";
            }
        }
    }

    public string FullName => _filename;

    public string Type => "RasterNet Raster File";

    public void Dispose()
    {
        _class = null;
    }

    public Task<object?> GetInstanceAsync()
    {
        if (_class == null)
        {
            // AddRasterFile returns null for files RasterNet cannot read (e.g. unsupported compressions).
            var layer = new Dataset().AddRasterFile(_filename);
            _class = layer?.Class as IRasterClass;
        }
        return Task.FromResult<object?>(_class);
    }

    public Task<IExplorerFileObject?> CreateInstance(IExplorerObject parent, string filename)
    {
        try
        {
            if (!new FileInfo(filename).Exists)
            {
                return Task.FromResult<IExplorerFileObject?>(null);
            }
        }
        catch
        {
            return Task.FromResult<IExplorerFileObject?>(null);
        }
        return Task.FromResult<IExplorerFileObject?>(new RasterNetFileExplorerObject(parent, filename));
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache != null && cache.Contains(FullName))
        {
            return cache[FullName];
        }

        try
        {
            if (!new FileInfo(FullName).Exists)
            {
                return null;
            }

            var explorerObject = new RasterNetFileExplorerObject(new NullParentExplorerObject(), FullName);
            if (await explorerObject.GetInstanceAsync() is IRasterClass)
            {
                cache?.Append(explorerObject);
                return explorerObject;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
