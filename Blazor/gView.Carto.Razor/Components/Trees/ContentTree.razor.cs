using gView.Carto.Core.Abstractions;
using gView.Carto.Core.Models.Tree;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.UI;

namespace gView.Carto.Razor.Components.Trees
{
    public partial class ContentTree
    {
        private ICartoApplicationScopeService? _cartoScope;

        private Task SetAppScope(ICartoApplicationScopeService cartoApplicationScopeService)
        {
            _cartoScope = cartoApplicationScopeService;

            InitEvents();

            return Rebuild();
        }

        private Task OnCartoDocumentLoadedAsync(ICartoDocument arg)
        {
            return Rebuild();
        }

        private async Task Rebuild()
        {
            if (_cartoScope == null)
            {
                return;
            }

            TreeNodes.Clear();

            var map = _cartoScope.Document?.Map;

            Rebuild(map, null);

            fullReloadKey = Guid.NewGuid();
            await _cartoScope.EventBus.FireRefreshContentTreeAsync();
        }

        private void Rebuild(IMap? map, TocTreeNode? parentTreeNode)
        {
            if (map?.TOC?.Elements != null)
            {
                foreach (var tocElement in map.TOC.Elements.Where(e => e.ParentGroup == parentTreeNode?.TocElement))
                {
                    TocTreeNode? childTreeNode = tocElement.ElementType switch
                    {
                        TOCElementType.OpenedGroup => new TocParentNode(tocElement),
                        TOCElementType.ClosedGroup => new TocParentNode(tocElement),
                        TOCElementType.Layer => new TocLayerNode(tocElement),
                        TOCElementType.Legend => new TocLegendNode(tocElement),
                        _ => null,
                    };

                    if (childTreeNode != null)
                    {
                        if (parentTreeNode == null)
                        {
                            TreeNodes.Add(childTreeNode);
                        }
                        else
                        {
                            parentTreeNode.Children = parentTreeNode.Children ?? new HashSet<TocTreeNode>();
                            parentTreeNode.Children.Add(childTreeNode);
                        }
                    }

                    if (childTreeNode is TocParentNode)
                    {
                        Rebuild(map, childTreeNode);
                    }
                }
            }
        }

        private Task RefreshContentTree()
            => this.InvokeAsync(() =>
            {
                StateHasChanged();
            });

        private void InitEvents()
        {
            EventBus.OnCartoDocumentLoadedAsync += OnCartoDocumentLoadedAsync;
            EventBus.OnRefreshContentTreeAsync += RefreshContentTree;
        }

        private void RelaseEvents()
        {
            EventBus.OnRefreshContentTreeAsync -= RefreshContentTree;
            EventBus.OnCartoDocumentLoadedAsync += OnCartoDocumentLoadedAsync;
        }

        public void Dispose()
        {

            RelaseEvents();

            foreach (var treeNode in TreeNodes)
            {
                treeNode?.Dispose();
            }
        }
    }
}
