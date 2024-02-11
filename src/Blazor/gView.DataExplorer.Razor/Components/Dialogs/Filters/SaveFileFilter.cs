using gView.Framework.DataExplorer.Abstraction;
using System.Text.RegularExpressions;

namespace gView.DataExplorer.Razor.Components.Dialogs.Filters;

public class SaveFileFilter : ExplorerDialogFilter
{
    private readonly Regex _regex;

    public SaveFileFilter(string name = "", string filter = "*.*")
        : base($"{name} file ({filter})".Trim())
    {
        this.FileFilter = filter;
        _regex = new Regex("^" + Regex.Escape(filter)
                                   .Replace("\\*", ".*")
                                   .Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
    }

    public override Task<bool> Match(IExplorerObject exObject)
    {
        try
        {
            return Task.FromResult(Directory.Exists(exObject.FullName));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public override async Task<bool> CanOverWrite(IExplorerObject exObject)
    {
        if (exObject?.ParentExplorerObject is null)
        {
            return false;
        }

        if (await Match(exObject.ParentExplorerObject) == false)
        {
            return false;
        }

        return exObject.GetType().Name == "FileObject"
            && _regex.IsMatch(exObject.Name);
    }

    public override string FileFilter { get; }

    public override IEnumerable<IExplorerObject> FilterExplorerObjects(IEnumerable<IExplorerObject> explorerObjects)
    {
        return explorerObjects
                    .Where(e => e.GetType().Name switch
                    {
                        "ComputerObject" => true,
                        "DriveObject" => true,
                        "MappedDriveObject" => true,
                        "DirectoryObject" => true,
                        "FileObject" => _regex.IsMatch(e.Name),
                        _ => false
                    });
    }
}
