using gView.Carto.Core.Abstraction;
using gView.Framework.Core.UI;
using System.Linq;

namespace gView.Carto.Razor.Components.Trees;

public class TocParentNode : TocTreeNode, ITocParentNode
{
    public TocParentNode(ITocElement tocElement) : base(tocElement)
    {
    }

    public override bool Expanded
    {
        get => base.Value?.ElementType switch
        {
            TocElementType.ClosedGroup => false,
            TocElementType.OpenedGroup => true,
            _ => false
        };
        set
        {
            base.Value?.OpenCloseGroup(value);
        }
    }

    public override bool Selected
    {
        get
        {
            return base.Value?.LayerVisible ?? false;
        }
        set
        {
            if (base.Value is not null)
            {
                base.Value.LayerVisible = value;
            }
        }
    }
}
