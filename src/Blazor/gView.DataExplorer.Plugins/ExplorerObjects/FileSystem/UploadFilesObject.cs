using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.FileSystem;

[RegisterPlugIn("00A6C972-8553-49C8-B8C4-1361D51F0B0A")]
[AuthorizedPlugin(RequireAdminRole = true)]
internal class UploadFilesObject : IExplorerObject,
                                   IExplorerObjectCreatable
{
    #region IExplorerObject

    public string Name => "Upload Files";

    public string FullName => "File Upload";

    public string? Type => "FileUpload";

    public string Icon => "basic:cloud-upload";

    public IExplorerObject? ParentExplorerObject => null;

    public Type? ObjectType => null;

    public int Priority => 99;

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
        => Task.FromResult<IExplorerObject?>(null);

    public void Dispose()
    {

    }

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);


    #endregion

    #region IExplorerObjectCreatable

    public bool CanCreate(IExplorerObject parentExObject)
        => parentExObject is DirectoryObject
        || parentExObject is MappedDriveObject;

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScopeService scope, IExplorerObject parentExObject)
    {
        var model = await scope.ShowModalDialog(
                   typeof(Razor.Components.Dialogs.UploadFilesDialog),
                   this.Name,
                   new UploadFilesModel()
                   {
                       TargetFolder = parentExObject.FullName
                   });
        
        if (model == null)
        {
            return null;
        }

        await scope.ForceContentRefresh();
        return null;
    }

    #endregion
}
