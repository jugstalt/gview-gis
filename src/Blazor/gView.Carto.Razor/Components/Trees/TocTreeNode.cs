using gView.Carto.Core.Abstraction;
using gView.Framework.Core.UI;
using MudBlazor;

namespace gView.Carto.Razor.Components.Trees;

abstract public class TocTreeNode : TreeItemData<ITocElement>, IDisposable, ITocTreeNode
{
    private readonly List<TocTreeNode> _children = new();

    protected TocTreeNode(ITocElement tocElement)
    {
        Value = tocElement;
        Text = Value.Name ?? string.Empty;

        Children = _children;
    }

    public override bool HasChildren => _children.Count > 0;

    public bool IsSelected { get; set; }

    public void AddChild(TocTreeNode child)
    {
        _children.Add(child);
    }

    public virtual void Dispose()
    {
        foreach (var child in _children)
        {
            child.Dispose();
        }

        _children.Clear();
    }
}
