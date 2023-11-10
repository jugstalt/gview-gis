using gView.Framework.UI;

namespace gView.Carto.Core.Models.Tree;

public class TocLegendNode : TocTreeNode
{
    public TocLegendNode(ITOCElement tocElement) : base(tocElement) 
    {
        this.Text = "Legende";
    }
}
