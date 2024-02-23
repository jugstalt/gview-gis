using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;

namespace gView.DataExplorer.Razor.Components.Dialogs.Filters;
internal class OpenFolderFilter : ExplorerDialogFilter
{
    public OpenFolderFilter()
        : base("Folder")
    {
    }

    public override Task<bool> Match(IExplorerObject exObject)
    {
        return Task.FromResult(
            exObject.Type == "Directory" 
            || exObject.Type?.StartsWith("Environment Variable: ") == true);
    }

    public override IEnumerable<IExplorerObject> FilterExplorerObjects(IEnumerable<IExplorerObject> explorerObjects)
    {
        return explorerObjects
                    .Where(e => e.GetType().Name switch
                        {
                            "ComputerObject" => true,
                            "DriveObject" => true,
                            "MappedDriveObject" => true,
                            "EnvironmentVariableDrive" => true,
                            "DirectoryObject" => true,
                            _ => false
                        });
    }
}
