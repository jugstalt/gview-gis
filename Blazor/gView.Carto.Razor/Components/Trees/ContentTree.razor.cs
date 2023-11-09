using gView.Carto.Core.Abstractions;
using gView.Carto.Core.Models.Tree;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.UI;

namespace gView.Carto.Razor.Components.Trees
{
    public partial class ContentTree
    {
        private ICartoApplicationScopeService? _cartoScope;
        private void SetAppScope(ICartoApplicationScopeService cartoApplicationScopeService)
        {
            _cartoScope = cartoApplicationScopeService;

            _cartoScope.EventBus.OnCartoDocumentLoadedAsync += OnCartoDocumentLoadedAsync;
        }

        private Task OnCartoDocumentLoadedAsync(ICartoDocument arg)
        {
            if(_cartoScope == null)
            {
                return Task.CompletedTask;
            }

            var map = _cartoScope.Document?.Map;

            if (map?.TOC.Elements != null)
            {
                foreach (var tocElement in map.TOC.Elements)
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
                        TreeNodes.Add(childTreeNode);
                        if (tocElement.ParentGroup == null)
                        {
                            this.Children.Add(childTreeNode);
                        }
                        else
                        {
                            var parentTocTreeNode = TreeNodes.FirstOrDefault(t => t.TocElement == tocElement.ParentGroup);
                            if (parentTocTreeNode != null)
                            {
                                parentTocTreeNode.Children = parentTocTreeNode.Children ?? new HashSet<TocTreeNode>();
                                parentTocTreeNode.Children.Add(childTreeNode);
                            }
                        }
                    }
                }
            }

            return _cartoScope.EventBus.FireRefreshContentTreeAsync();
        }
    }
}
