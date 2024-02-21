using gView.Blazor.Core.Exceptions;
using gView.DataExplorer.Core.Extensions;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Base.ContextTools;
using gView.DataSources.Fdb.PostgreSql;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.Db;
using gView.Framework.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.PostgreSql;

public class PostgreSqlExplorerObject : ExplorerParentObject<PostgreSqlExplorerGroupObject>,
                                    IExplorerSimpleObject,
                                    IExplorerObjectDeletable,
                                    IExplorerObjectRenamable,
                                    IExplorerObjectCommandParameters,
                                    ISerializableExplorerObject
{
    private string _server = String.Empty, _errMsg = String.Empty;
    private DbConnectionString? _dbConnectionString = null;
    private IEnumerable<IExplorerObjectContextTool>? _contextTools = null;

    public PostgreSqlExplorerObject(int priority) : base() { }

    public PostgreSqlExplorerObject(PostgreSqlExplorerGroupObject parent, string server, DbConnectionString dbConnectionString)
         : base(parent, 1)
    {
        _server = server;
        _dbConnectionString = dbConnectionString;

        _contextTools = [new UpdateConnectionString()];
    }

    internal string ConnectionString => _dbConnectionString?.ConnectionString ?? String.Empty;

    #region IUpdateConnectionString

    public DbConnectionString GetDbConnectionString()
    {
        if (_dbConnectionString == null)
        {
            throw new GeneralException("Error: connection string not set already for this item");
        }

        return _dbConnectionString.Clone();
    }
    public Task<bool> UpdateDbConnectionString(DbConnectionString dbConnnectionString)
    {
        ConfigConnections connStream = ConfigConnections.Create(
                this.ConfigStorage(),
                "postgrefdb",
                "546B0513-D71D-4490-9E27-94CD5D72C64A"
            );
        connStream.Add(_server, dbConnnectionString.ToString());

        _dbConnectionString = dbConnnectionString;

        return Task.FromResult(true);
    }

    #endregion

    #region IExplorerObjectContextTools

    public IEnumerable<IExplorerObjectContextTool> ContextTools => _contextTools ?? Array.Empty<IExplorerObjectContextTool>();

    #endregion

    #region IExplorerObject Members

    public string Name => _server;

    public string FullName => @$"Databases\PostgreFDBConnections\{_server}";

    public string Type => "Postgre Feature Database";

    public string Icon => "basic:database";

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);


    #endregion

    async private Task<string[]> DatasetNames()
    {

        try
        {
            pgFDB fdb = new pgFDB();
            if (!await fdb.Open(this.ConnectionString))
            {
                _errMsg = fdb.LastErrorMessage;
                throw new GeneralException(_errMsg);
            }
            string[] ds = await fdb.DatasetNames();

            if (ds == null)
            {
                if (String.IsNullOrEmpty(fdb.LastErrorMessage))
                {
                    return Array.Empty<string>();
                }
                _errMsg = fdb.LastErrorMessage;
                throw new GeneralException(_errMsg);
            }

            string[] dsMod = new string[ds.Length];

            int i = 0;
            foreach (string dsname in ds)
            {
                var isImageDatasetResult = await fdb.IsImageDataset(dsname);
                string imageSpace = isImageDatasetResult.imageSpace;
                if (isImageDatasetResult.isImageDataset)
                {
                    dsMod[i++] = "#" + dsname;
                }
                else
                {
                    dsMod[i++] = dsname;
                }
            }

            fdb.Dispose();

            return dsMod;
        }
        catch (Exception ex)
        {
            _errMsg = ex.Message;
            throw;
        }
    }

    #region IExplorerParentObject Members

    async public override Task<bool> Refresh()
    {
        await base.Refresh();
        string[] ds = await DatasetNames();
        if (ds == null)
        {
            throw new GeneralException(_errMsg);
        }
        else
        {
            foreach (string dsname in ds)
            {
                if (dsname == String.Empty)
                {
                    continue;
                }

                base.AddChildObject(new PostgreSqlDatasetExplorerObject(this, dsname));
            }
        }

        return true;
    }
    #endregion

    #region IExplorerObjectCommandParameters Members

    public Dictionary<string, string> Parameters
    {
        get
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Connection String", this.ConnectionString);
            return parameters;
        }
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache?.Contains(FullName) == true)
        {
            return cache[FullName];
        }

        PostgreSqlExplorerGroupObject? group = new PostgreSqlExplorerGroupObject();
        if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2)
        {
            return null;
        }

        group = (PostgreSqlExplorerGroupObject?)((cache?.Contains(group.FullName) == true) ? cache[group.FullName] : group);

        if (group != null)
        {
            foreach (IExplorerObject exObject in await group.ChildObjects())
            {
                if (exObject.FullName == FullName)
                {
                    cache?.Append(exObject);
                    return exObject;
                }
            }
        }

        return null;
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        ConfigConnections stream = ConfigConnections.Create(
                    this.ConfigStorage(),
                    "postgrefdb",
                    "546B0513-D71D-4490-9E27-94CD5D72C64A"
                );
        stream.Remove(_server);


        if (ExplorerObjectDeleted != null)
        {
            ExplorerObjectDeleted(this);
        }

        return Task.FromResult(true);
    }

    #endregion

    #region IExplorerObjectRenamable Member

    public event ExplorerObjectRenamedEvent? ExplorerObjectRenamed;

    public Task<bool> RenameExplorerObject(string newName)
    {
        ConfigConnections stream = ConfigConnections.Create(
                this.ConfigStorage(),
                "postgrefdb",
                "546B0513-D71D-4490-9E27-94CD5D72C64A"
            );

        bool ret = stream.Rename(_server, newName);

        if (ret == true)
        {
            _server = newName;
            if (ExplorerObjectRenamed != null)
            {
                ExplorerObjectRenamed(this);
            }
        }
        return Task.FromResult(ret);
    }

    #endregion
}

////[RegisterPlugIn("9B5B718C-2ECA-47ee-851F-9D33E3D82C55")]
//public class SqlFDBTileGridClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectCreatable
//{
//    public SqlFDBTileGridClassExplorerObject() : base(null, null, 1) { }

//    #region IExplorerObject Member

//    public string Name
//    {
//        get { return String.Empty; }
//    }

//    public string FullName
//    {
//        get { return String.Empty; }
//    }

//    public string Type
//    {
//        get { return "Tile Grid Class"; }
//    }

//    public IExplorerIcon Icon
//    {
//        get
//        {
//            return new gView.DataSources.Fdb.UI.MSAccess.AccessFDBRasterIcon();
//        }
//    }

//    public Task<object> GetInstanceAsync()
//    {
//        return Task.FromResult<object>(null);
//    }

//    public new int Priority { get { return 1; } }

//    #endregion

//    #region IDisposable Member

//    public void Dispose()
//    {

//    }

//    #endregion

//    #region ISerializableExplorerObject Member

//    public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
//    {
//        return Task.FromResult<IExplorerObject>(null);
//    }

//    #endregion

//    #region IExplorerObjectCreatable Member

//    public bool CanCreate(IExplorerObject parentExObject)
//    {
//        if (parentExObject is DatasetExplorerObject)
//        {
//            return true;
//        }

//        return false;
//    }

//    async public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
//    {
//        if (!(parentExObject is DatasetExplorerObject))
//        {
//            return null;
//        }

//        IFeatureDataset dataset = await ((DatasetExplorerObject)parentExObject).GetInstanceAsync() as IFeatureDataset;
//        if (dataset == null || !(dataset.Database is SqlFDB))
//        {
//            return null;
//        }

//        FormNewTileGridClass dlg = new FormNewTileGridClass();
//        if (dlg.ShowDialog() != DialogResult.OK)
//        {
//            return null;
//        }

//        CreateTileGridClass creator = new CreateTileGridClass(
//            dlg.GridName,
//            dataset, dlg.SpatialIndexDefinition,
//            dlg.RasterDataset,
//            dlg.TileSizeX, dlg.TileSizeY, dlg.ResolutionX, dlg.ResolutionY, dlg.Levels,
//            dlg.TileCacheDirectory,
//            dlg.TileGridType);
//        creator.CreateTiles = dlg.GenerateTileCache;
//        creator.TileLevelType = dlg.TileLevelType;
//        creator.CreateLevels = dlg.CreateLevels;

//        FormTaskProgress progress = new FormTaskProgress();
//        progress.ShowProgressDialog(creator, creator.RunTask());

//        IDatasetElement element = await dataset.Element(dlg.GridName);
//        return new FeatureClassExplorerObject(
//            parentExObject as DatasetExplorerObject,
//            parentExObject.Name,
//            element);
//    }

//    #endregion
//}
