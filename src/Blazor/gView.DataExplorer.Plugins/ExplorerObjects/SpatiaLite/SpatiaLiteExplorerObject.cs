using gView.Blazor.Core.Exceptions;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.FileSystem;
using gView.DataSources.SpatiaLite;
using gView.Framework.Common.Extensions;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.SpatiaLite;

[RegisterPlugIn("29172403-2f3e-485a-bb06-d5039fe89cbd")]
public class SpatiaLiteExplorerObject : ExplorerParentObject<IExplorerObject, IFeatureDataset>,
                                        IExplorerFileObject,
                                        ISerializableExplorerObject,
                                        IExplorerObjectCreatable,
                                        IExplorerObjectDeletable
{
    private string _filename = "";
    private SpatiaLiteDataset? _dataset;

    // High priority so the "Create new" ribbon lists "SpatiaLite / GeoPackage" after the
    // built-in creatables (Directory, FileUpload, SQLite Feature Database).
    public SpatiaLiteExplorerObject() : base(1000) { }

    private SpatiaLiteExplorerObject(IExplorerObject parent, string filename)
        : base(parent, 2)
    {
        _filename = filename;
    }

    #region IExplorerFileObject

    public string Filter => "*.sqlite|*.db|*.sqlite3|*.gpkg";

    async public Task<IExplorerFileObject?> CreateInstance(IExplorerObject parent, string filename)
    {
        try
        {
            if (!new FileInfo(filename).Exists)
            {
                return null;
            }

            var dataset = new SpatiaLiteDataset();
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

        return new SpatiaLiteExplorerObject(parent, filename);
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

    public string Type => "SpatiaLite / GeoPackage";

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
            _dataset = new SpatiaLiteDataset();
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

        var dataset = new SpatiaLiteDataset();
        await dataset.SetConnectionString(_filename);
        if (!await dataset.Open())
        {
            return false;
        }

        foreach (var element in (await dataset.Elements()).OrEmpty())
        {
            if (element.Class is IFeatureClass)
            {
                base.AddChildObject(new SpatiaLiteFeatureClassExplorerObject(this, element));
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
            "Create SpatiaLite / GeoPackage",
            new gView.DataExplorer.Razor.Components.Dialogs.Models.InputBoxModel()
            {
                Value = "",
                Icon = this.Icon,
                Name = this.Type ?? String.Empty,
                Label = "Name",
                Prompt = "Enter a name. A '.gpkg' extension creates a GeoPackage, anything else a SpatiaLite database (.sqlite)."
            });

        if (String.IsNullOrEmpty(model?.Value))
        {
            return null;
        }

        var name = model.Value.Trim();
        if (!HasKnownExtension(name))
        {
            name += ".sqlite";
        }

        var filename = Path.Combine(parentExObject.FullName, name);

        var dataset = new SpatiaLiteDataset();
        if (!dataset.Create(filename))
        {
            throw new GeneralException(dataset.LastErrorMessage);
        }

        return new SpatiaLiteExplorerObject(parentExObject, filename);
    }

    private static bool HasKnownExtension(string name)
        => name.EndsWith(".gpkg", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".sqlite3", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".db", StringComparison.OrdinalIgnoreCase);

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
