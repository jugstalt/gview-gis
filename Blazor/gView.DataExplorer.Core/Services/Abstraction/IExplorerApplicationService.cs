using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using System.Collections.Generic;

namespace gView.DataExplorer.Core.Services.Abstraction;

public interface IExplorerApplicationService : IApplication
{
    public IExplorerObject RootExplorerObject { get; }

    public string GetConfigFilename(params string[] paths);
    public IEnumerable<string> GetConfigFiles(params string[] paths);
}
