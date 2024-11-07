using gView.Carto.Core.Abstraction;
using gView.Framework.Core.UI;

namespace gView.Carto.Razor.Components.Trees;

public class TocLegendNode : TocTreeNode, ITocLegendNode
{
    public TocLegendNode(ITocElement tocElement) : base(tocElement) 
    {
        this.Text = "Legende";
    }
}
