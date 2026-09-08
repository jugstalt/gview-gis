using gView.Blazor.Core.Exceptions;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Extensions;
using gView.DataExplorer.Plugins.ExplorerObjects.FileSystem;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.SqLite;

[RegisterPlugIn("A4F900EC-C5E4-4518-BAB9-213AF660E8F1")]
internal class SqLiteFdbExplorerObject : ExplorerParentObject<IExplorerObject>,
                                         IExplorerFileObject,
                                         IExplorerObjectCustomContentValues,
                                         IExplorerObjectCommandParameters,
                                         ISerializableExplorerObject,
                                         IExplorerObjectDeletable,
                                         IExplorerObjectCreatable
{
    private string _filename = "", _errMsg = "";

    public SqLiteFdbExplorerObject() : base() { }
    public SqLiteFdbExplorerObject(IExplorerObject parent, string filename)
        : base(parent, 2)
    {
        _filename = filename;
    }

    #region IExplorerObject Members

    public string Filter
    {
        // classic *.fdb plus the GeoPackage / SpatiaLite flavored FDB files
        get { return "*.fdb|*.fdb.gpkg|*.fdb.sqlite"; }
    }

    public string Name
    {
        get
        {
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(_filename);
                return fi.Name;
            }
            catch { return ""; }
        }
    }

    public string FullName
    {
        get
        {
            return _filename;
        }
    }

    public string Type
    {
        get { return "SQLite Feature Database"; }
    }

    public string Icon => "basic:database";

    public Task<object?> GetInstanceAsync()
    {
        return Task.FromResult<object?>(null);
    }

    async public Task<IExplorerFileObject?> CreateInstance(IExplorerObject parent, string filename)
    {
        string f = filename.ToLower();
        if (!SqliteFdbFile.IsFdbFileName(f))
        {
            return null;
        }

        try
        {
            if (!(new FileInfo(f).Exists))
            {
                return null;
            }

            using (SQLiteFDB fdb = new SQLiteFDB())
            {
                if (!await fdb.Open(f) || !await fdb.IsValidAccessFDB())
                {
                    return null;
                }
            }
        }
        catch { return null; }

        return new SqLiteFdbExplorerObject(parent, filename);
    }
    #endregion

    #region IExplorerObjectCustomContentValues

    public IDictionary<string, object?> GetCustomContentValues()
        => _filename.GetFileProperties();

    #endregion

    async private Task<string[]> DatasetNames()
    {
        try
        {
            SQLiteFDB fdb = new SQLiteFDB();
            if (!await fdb.Open(_filename))
            {
                _errMsg = fdb.LastErrorMessage;
                return Array.Empty<string>();
            }
            string[] ds = await fdb.DatasetNames();
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
            if (ds == null)
            {
                _errMsg = fdb.LastErrorMessage;
            }

            fdb.Dispose();

            return dsMod;
        }
        catch
        {
            throw;
        }
    }

    #region IExplorerParentObject Members

    async public override Task<bool> Refresh()
    {
        await base.Refresh();
        string[] ds = await DatasetNames();
        if (ds != null)
        {
            foreach (string dsname in ds)
            {
                if (dsname == "")
                {
                    continue;
                }

                base.AddChildObject(new SqLiteFdbDatasetExplorerObject(this, _filename, dsname));
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
            parameters.Add("SQLiteFDB", _filename);
            return parameters;
        }
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        IExplorerObject? obj = (cache != null && cache.Contains(FullName)) ?
            cache[FullName] :
            await CreateInstance(Parent, FullName);

        if (obj != null)
        {
            cache?.Append(obj);
        }

        return obj;
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        if (parentExObject is DirectoryObject ||
            parentExObject is DriveObject)
        {
            return true;
        }

        return false;
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScopeService scope,
                                                                  IExplorerObject parentExObject)
    {
        if (!CanCreate(parentExObject))
        {
            return null;
        }

        var model = await scope.ShowModalDialog(
            typeof(gView.DataExplorer.Razor.Components.Dialogs.NewSqliteFdbDialog),
            "Create",
            new NewSqliteFdbModel()
            {
                Name = "",
                Directory = parentExObject.FullName,
                SpatiaLiteAvailable = SqliteFdbFile.SpatiaLiteAvailable,
            });

        if (!String.IsNullOrWhiteSpace(model?.Name))
        {
            var name = SqliteFdbFile.EnsureExtension(model.Name.Trim(), model.Storage);
            var filename = Path.Combine(parentExObject.FullName, name);

            if (File.Exists(filename))
            {
                throw new GeneralException($"'{name}' already exists in this folder.");
            }

            var fdb = new SQLiteFDB();
            if (!fdb.Create(filename))
            {
                throw new GeneralException(
                    String.IsNullOrEmpty(fdb.LastErrorMessage)
                        ? $"Could not create '{name}'."
                        : fdb.LastErrorMessage);
            }

            return new SqLiteFdbExplorerObject(parentExObject, filename);
        }

        return null;
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted = null;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        // release any pooled SQLite / mod_spatialite handle that would keep the file locked
        SQLiteFDB.ReleaseFileHandles();

        try
        {
            foreach (var path in new[] { _filename, _filename + "-wal", _filename + "-shm", _filename + "-journal" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            ExplorerObjectDeleted?.Invoke(this);
            return Task.FromResult(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new GeneralException(
                $"Cannot delete '{Name}' - the file is still open. " +
                "Close every dataset / map that uses it (in gView or another program such as QGIS), then try again.");
        }
    }

    #endregion
}
