using gView.Blazor.Models.Content;
using gView.DataExplorer.Core.Models.Content;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerTabPages;

[RegisterPlugIn("EE9D26E9-6591-4BD1-9A2D-604157575277")]
internal class TileBundleContentPage : IExplorerTabPage
{
    public Type RazorComponent => typeof(Razor.Components.Contents.TileBundleContent);

    public string Title => "Tile Bundle";

    public int SortOrder => 0;

    public IExplorerObject? GetExplorerObject()
    {
        throw new NotImplementedException();
    }

    public void OnHide()
    {

    }

    public Task<bool> OnShow()
    {
        return Task.FromResult(true);
    }

    public Task<IContentItemResult> RefreshContents(bool force = false)
    {
        return Task.FromResult<IContentItemResult>(ContentItemResult.Empty);
    }

    public Task SetExplorerObjectAsync(IExplorerObject? value)
    {
        return Task.CompletedTask;
    }

    public Task<bool> ShowWith(IExplorerObject? exObject)
    {
        if (exObject == null)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(TypeHelper.Match(exObject.ObjectType, typeof(TileBundleContent)));
    }
}
