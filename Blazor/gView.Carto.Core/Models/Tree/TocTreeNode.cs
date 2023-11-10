using gView.Blazor.Core.Extensions;
using gView.Blazor.Models.Tree;
using gView.Framework.UI;

namespace gView.Carto.Core.Models.Tree;

public class TocTreeNode : TreeItem<TocTreeNode>
{
    private readonly ITOCElement _tocElement;

    public TocTreeNode(ITOCElement tocElement)
    {
        _tocElement = tocElement;

        if (_tocElement != null)
        {
            this.Text = _tocElement?.Name ?? string.Empty;
        }
    }

    public ITOCElement TocElement => _tocElement;

    public override void Dispose()
    {
        if (Children is not null)
        {
            foreach (var childNode in Children)
            {
                childNode.Dispose();
            }

            Children = null;
        }
    }
}
