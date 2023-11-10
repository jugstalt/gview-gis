using gView.Framework.Data;
using gView.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace gView.Carto.Core.Models.Tree;

public class TocLayerNode : TocTreeNode
{
    public TocLayerNode(ITocElement tocElement) : base(tocElement)
    {
        this.Icon = "basic:checkbox-checked";

        if(tocElement.Layers?.Any(l=> l is IFeatureLayer) == true)
        {
            //this.Children = new HashSet<TocTreeNode>();
            //this.Children.Add(new TocLegendNode(tocElement));
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
