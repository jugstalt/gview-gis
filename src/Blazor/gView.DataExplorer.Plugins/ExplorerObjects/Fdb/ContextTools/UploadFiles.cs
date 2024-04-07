using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;
internal class UploadFiles : IExplorerObjectContextTool
{
    public string Name => "Upload Files...";

    public string Icon => "basic:cloud-upload";

    public bool IsEnabled(IExplorerApplicationScopeService scope, IExplorerObject exObject)
        => true;

    public Task<bool> OnEvent(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        throw new NotImplementedException();
    }
}
