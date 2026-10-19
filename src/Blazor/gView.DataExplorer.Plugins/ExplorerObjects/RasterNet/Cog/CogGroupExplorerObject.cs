using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.WebServices;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.RasterNet.Cog;

/// <summary>
/// WebServices\Cloud Optimized GeoTIFF: GeoTIFF/COG files on a web server or in object storage, read by RasterNet with
/// range requests. Every connection is one raster, stored by name and URL.
/// </summary>
[RegisterPlugIn("6B6394D7-9C39-4325-A825-12777D73CC23")]
public class CogGroupExplorerObject : ExplorerParentObject, IWebServicesExplorerGroupObject
{
    public CogGroupExplorerObject() : base() { }

    #region IExplorerObject Members

    public string Name => "Cloud Optimized GeoTIFF (COG)";

    public string FullName => @"WebServices\COG";

    public string Type => "Cloud Optimized GeoTIFF Connections";

    public string Icon => "webgis:georef-image";

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region IExplorerParentObject Members

    async public override Task<bool> Refresh()
    {
        await base.Refresh();
        base.AddChildObject(new CogNewConnectionExplorerObject(this));

        var connections = CogConnections.Open(this).Connections;
        foreach (string name in connections.Keys)
        {
            base.AddChildObject(new CogConnectionExplorerObject(this, name, connections[name]));
        }
        return true;
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache?.Contains(FullName) == true)
        {
            return Task.FromResult(cache[FullName]);
        }
        if (FullName == this.FullName)
        {
            var exObject = new CogGroupExplorerObject();
            cache?.Append(exObject);
            return Task.FromResult<IExplorerObject?>(exObject);
        }
        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IWebServicesExplorerGroupObject

    public void SetParentExplorerObject(IExplorerObject parentExplorerObject)
    {
        base.Parent = parentExplorerObject;
    }

    #endregion
}
