using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Esri;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.Db;
using gView.Framework.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.MsSqlSpatial.Sde;

[gView.Framework.system.RegisterPlugIn("55AF4451-7A67-48C8-8F41-F2E3A6DA7EB1")]
public class MsSqlSpatialSdeExplorerGroupObject : ExplorerParentObject, IEsriGroupExplorerObject
{
    public MsSqlSpatialSdeExplorerGroupObject()
        : base(null, null, 0)
    { }

    #region IExplorerGroupObject Members

    public string Icon => "basic:edit-database";

    #endregion

    #region IExplorerObject Members

    public string Name => "MsSql Spatial Geometry (ArcSde)";

    public string FullName => @"ESRI\MsSqlSpatialSde";
    public string Type => "MsSql Spatial ArcSde Connection";
    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region IExplorerParentObject Members

    async public override Task<bool> Refresh()
    {
        await base.Refresh();
        base.AddChildObject(new MsSqlSpatialSdeNewConnectionObject(this));

        ConfigConnections conStream = new ConfigConnections("mssql-sde", "546B0513-D71D-4490-9E27-94CD5D72C64A");
        Dictionary<string, string> DbConnectionStrings = conStream.Connections;

        foreach (string DbConnName in DbConnectionStrings.Keys)
        {
            DbConnectionString dbConn = new DbConnectionString();
            dbConn.FromString(DbConnectionStrings[DbConnName]);
            base.AddChildObject(new MsSqlSpatialSdeExplorerObject(this, DbConnName, dbConn));
        }

        return true;
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache != null && cache.Contains(FullName))
        {
            return Task.FromResult<IExplorerObject?>(cache[FullName]);
        }

        if (this.FullName == FullName)
        {
            MsSqlSpatialSdeExplorerGroupObject exObject = new MsSqlSpatialSdeExplorerGroupObject();
            cache?.Append(exObject);
            return Task.FromResult<IExplorerObject?>(exObject);
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IEsriGroupExplorerObject

    public void SetParentExplorerObject(IExplorerObject parentExplorerObject)
    {
        base._parent = parentExplorerObject;
    }

    #endregion
}
