using gView.Carto.Core.Abstraction;
using gView.Framework.Core.UI;
using MudBlazor;

namespace gView.Carto.Razor.Components.Trees;

abstract public class TocTreeNode : TreeItemData<ITocElement>, IDisposable, ITocTreeNode
{
    public TocTreeNode(ITocElement tocElement)
    {
        this.Value = tocElement;

        this.Text = this.Value.Name ?? string.Empty;
    }

    public override bool HasChildren
    {
        get => this.Children?.Any() ?? false;
    }

    public bool IsSelected { get; set; }

    public virtual void Dispose()
    {
        if (Children is not null)
        {
            foreach (TocTreeNode childNode in Children)
            {
                childNode.Dispose();
            }

            Children = null;
        }
    }
}
