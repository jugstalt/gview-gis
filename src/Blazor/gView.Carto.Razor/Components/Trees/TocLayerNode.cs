using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Extensions;
using gView.Framework.Core.UI;
using MudBlazor;

namespace gView.Carto.Razor.Components.Trees;

public class TocLayerNode : TocTreeNode, ITocLayerNode
{
    public TocLayerNode(ITocElement tocElement) : base(tocElement)
    {
        if (tocElement.HasLegendItems())
        {
            this.Children = new List<TreeItemData<ITocElement>>() 
            {
                new TocLegendNode(tocElement)
            };
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
