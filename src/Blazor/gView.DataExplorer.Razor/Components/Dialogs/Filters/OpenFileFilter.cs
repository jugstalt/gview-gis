﻿using gView.Framework.DataExplorer.Abstraction;
using System.Text.RegularExpressions;

namespace gView.DataExplorer.Razor.Components.Dialogs.Filters;
public class OpenFileFilter : ExplorerDialogFilter
{
    private readonly Regex _regex;

    public OpenFileFilter(string name = "", string filter = "*.*")
        : base($"{name} file ({filter})".Trim())
    {
        this.FileFilter = filter;
        _regex = new Regex("^" + Regex.Escape(filter)
                                   .Replace("\\*", ".*")
                                   .Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
    }

    public override Task<bool> Match(IExplorerObject exObject)
    {
        return Task.FromResult(DoesFilenameMatchFilter(exObject.Name));
    }

    public override IEnumerable<IExplorerObject> FilterExplorerObjects(IEnumerable<IExplorerObject> explorerObjects)
    {
        return explorerObjects
                    .Where(e => e.GetType().Name switch
                    {
                        "ComputerObject" => true,
                        "DriveObject" => true,
                        "EnvironmentVariableDrive" => true,
                        "MappedDriveObject" => true,
                        "DirectoryObject" => true,
                        "FileObject" => _regex.IsMatch(e.Name),
                        _ => false
                    });
    }

    public override string FileFilter { get; }

    private bool DoesFilenameMatchFilter(string filename)
    {
        return _regex.IsMatch(filename);
    }
}
