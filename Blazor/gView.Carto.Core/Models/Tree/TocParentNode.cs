using gView.Framework.UI;
using System.Linq;

namespace gView.Carto.Core.Models.Tree;

public class TocParentNode : TocTreeNode
{
    public TocParentNode(ITocElement tocElement) : base(tocElement)
    {
        this.Icon = "basic:checkbox-unchecked";
    }

    public override bool IsExpanded
    {
        get => base.TocElement?.ElementType switch
        {
            TocElementType.ClosedGroup => false,
            TocElementType.OpenedGroup => true,
            _ => false
        };
        set
        {
            base.TocElement?.OpenCloseGroup(value);
        }
    }

    public override bool IsSelected
    {
        get
        {
            return base.TocElement.LayerVisible;
        }
        set
        {
            base.TocElement.LayerVisible = value;
        }
    }
}
