using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;
internal class AddImages : IExplorerObjectContextTool
{
    public string Name => "Add Image Folder";

    public string Icon => "basic:folder-white";

    public bool IsEnabled(IApplicationScope scope, IExplorerObject exObject)
    {
        return true;
    }

    public Task<bool> OnEvent(IApplicationScope scope, IExplorerObject exObject)
    {
        throw new NotImplementedException();
    }
}
