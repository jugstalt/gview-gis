using gView.DataExplorer.Core.Services.Abstraction;
using gView.DataExplorer.Plugins.ExplorerObjects;
using gView.Framework.DataExplorer.Abstraction;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.DataExplorer.Plugins.Services;
internal class ExplorerDesktopApplicationService : IExplorerApplicationService
{
    private readonly IExplorerObject _rootExplorerObject;
    private readonly ExplorerDesktopApplicationServiceOptions _options;

    public ExplorerDesktopApplicationService(IOptions<ExplorerDesktopApplicationServiceOptions> options)
    {
        _rootExplorerObject = new StartObject();
        _options = options.Value;
    }

    #region IExplorerApplicationService

    public IExplorerObject RootExplorerObject => _rootExplorerObject;

    public string GetConfigFilename(params string[] paths)
    {
        var fileInfo = new FileInfo(Path.Combine(_options.ConfigRootPath, Path.Combine(paths)));

        if (fileInfo.Directory!=null && !fileInfo.Directory.Exists)
        {
            fileInfo.Directory.Create();
        }

        return fileInfo.FullName;
    }

    public IEnumerable<string> GetConfigFiles(params string[] paths)
    {
        var directoryInfo = new DirectoryInfo(Path.Combine(_options.ConfigRootPath, Path.Combine(paths)));

        if (!directoryInfo.Exists)
        {
            return Array.Empty<string>();
        }

        return directoryInfo.GetFiles().Select(f => f.FullName);
    }

    #endregion
}
