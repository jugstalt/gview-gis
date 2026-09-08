using gView.Blazor.Core.Exceptions;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.FileSystem;
using gView.DataSources.GeoPackage;
using gView.Framework.Common.Extensions;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.GeoPackage;

[RegisterPlugIn("7a684b04-8c38-4717-9527-a7248aa72b8d")]
public class GeoPackageExplorerObject : ExplorerParentObject<IExplorerObject, IFeatureDataset>,
                                        IExplorerFileObject,
                                        ISerializableExplorerObject,
                                        IExplorerObjectCreatable,
                                        IExplorerObjectDeletable
{
    private string _filename = "";
    private GeoPackageDataset? _dataset;

    // High priority so the "Create new" ribbon lists "GeoPackage" next to "SpatiaLite".
    public GeoPackageExplorerObject() : base(1000) { }

    private GeoPackageExplorerObject(IExplorerObject parent, string filename)
        : base(parent, 2)
    {
        _filename = filename;
    }

    #region IExplorerFileObject

    public string Filter => "*.gpkg";

    async public Task<IExplorerFileObject?> CreateInstance(IExplorerObject parent, string filename)
    {
        try
        {
            if (!new FileInfo(filename).Exists)
            {
                return null;
            }

            // *.fdb.gpkg is a gView SQLite feature database - handled by the FDB explorer object,
            // not the plain GeoPackage datasource.
            if (gView.DataSources.Fdb.SQLite.SqliteFdbFile.IsFdbFileName(filename))
            {
                return null;
            }

            var dataset = new GeoPackageDataset();
            await dataset.SetConnectionString(filename);
            if (!await dataset.Open())
            {
                return null;
            }
        }
        catch
        {
            return null;
        }

        return new GeoPackageExplorerObject(parent, filename);
    }

    #endregion

    #region IExplorerObject Members

    public string Name
    {
        get
        {
            try { return new FileInfo(_filename).Name; }
            catch { return ""; }
        }
    }

    public string FullName => _filename;

    public string Type => "GeoPackage";

    public string Icon => "basic:database";

    public override void Dispose()
    {
        base.Dispose();

        if (_dataset != null)
        {
            _dataset.Dispose();
            _dataset = null;
        }
    }

    async public Task<object?> GetInstanceAsync()
    {
        if (_dataset == null)
        {
            _dataset = new GeoPackageDataset();
            await _dataset.SetConnectionString(_filename);
            if (!await _dataset.Open())
            {
                _dataset.Dispose();
                _dataset = null;
            }
        }

        return _dataset;
    }

    #endregion

    #region IExplorerParentObject Members

    async public override Task<bool> Refresh()
    {
        await base.Refresh();

        var dataset = new GeoPackageDataset();
        await dataset.SetConnectionString(_filename);
        if (!await dataset.Open())
        {
            return false;
        }

        foreach (var element in (await dataset.Elements()).OrEmpty())
        {
            if (element.Class is IFeatureClass)
            {
                base.AddChildObject(new GeoPackageFeatureClassExplorerObject(this, element));
            }
        }

        return true;
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        IExplorerObject? obj = (cache != null && cache.Contains(FullName))
            ? cache[FullName]
            : await CreateInstance(Parent, FullName);

        if (obj != null)
        {
            cache?.Append(obj);
        }

        return obj;
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
        => parentExObject is DirectoryObject || parentExObject is DriveObject;

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScopeService scope,
                                                                  IExplorerObject parentExObject)
    {
        if (!CanCreate(parentExObject))
        {
            return null;
        }

        var model = await scope.ShowModalDialog(
            typeof(gView.DataExplorer.Razor.Components.Dialogs.InputBoxDialog),
            "Create GeoPackage",
            new gView.DataExplorer.Razor.Components.Dialogs.Models.InputBoxModel()
            {
                Value = "",
                Icon = this.Icon,
                Name = this.Type ?? String.Empty,
                Label = "Name",
                Prompt = "Enter a name. A '.gpkg' extension is added when you omit it. No native library needed."
            });

        if (String.IsNullOrEmpty(model?.Value))
        {
            return null;
        }

        var name = model.Value.Trim();
        if (!name.EndsWith(".gpkg", StringComparison.OrdinalIgnoreCase))
        {
            name += ".gpkg";
        }

        var filename = Path.Combine(parentExObject.FullName, name);

        var dataset = new GeoPackageDataset();
        if (!dataset.Create(filename))
        {
            throw new GeneralException(dataset.LastErrorMessage);
        }

        return new GeoPackageExplorerObject(parentExObject, filename);
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        try
        {
            new FileInfo(_filename).Delete();
            ExplorerObjectDeleted?.Invoke(this);

            return Task.FromResult(true);
        }
        catch
        {
            throw;
        }
    }

    #endregion
}
