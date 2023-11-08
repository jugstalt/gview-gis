using gView.Carto.Core.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.Carto.Core.Models.Tree;

public class TocMapNode : TocTreeNode
{
    private readonly ICartoApplicationScopeService _cartoScope;
    public TocMapNode(ICartoApplicationScopeService cartoScope)
    {
        _cartoScope = cartoScope;
        _cartoScope.EventBus.OnCartoDocumentLoadedAsync += OnCartoDocumentLoadedAsync;

        base.Text = _cartoScope.Document?.Map?.Name ?? "Map";
        base.Icon = "basic:globe-table";
    }

    private Task OnCartoDocumentLoadedAsync(Abstractions.ICartoDocument arg)
    {
        base.Text = _cartoScope.Document?.Map?.Name ?? "Map";

        return _cartoScope.EventBus.FireRefreshContentTreeAsync();
    }

    public override void Dispose()
    {
        _cartoScope.EventBus.OnCartoDocumentLoadedAsync -= OnCartoDocumentLoadedAsync;

        base.Dispose();
    }
}
