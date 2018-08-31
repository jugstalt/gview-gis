using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;

namespace gView.Framework.Data.Joins
{
    public interface IJoinPropertyPanel
    {
        object PropertyPanel(IFeatureLayerJoin join, IMapDocument mapDocument);
    }
}
