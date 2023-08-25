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

    public ExplorerDesktopApplicationService()
    {
        _rootExplorerObject = new StartObject();
    }

    #region IExplorerApplicationService

    public IExplorerObject RootExplorerObject => _rootExplorerObject;

    #endregion
}
