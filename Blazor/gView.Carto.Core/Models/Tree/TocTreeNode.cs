using gView.Blazor.Models.Tree;

namespace gView.Carto.Core.Models.Tree;

public class TocTreeNode : TreeItem<TocTreeNode>
{
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
