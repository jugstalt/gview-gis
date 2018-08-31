using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Symbology;

namespace gView.Framework.Symbology.UI
{
    public interface IPropertyPanel
    {
        object PropertyPanel(ISymbol symbol);
    }
}
