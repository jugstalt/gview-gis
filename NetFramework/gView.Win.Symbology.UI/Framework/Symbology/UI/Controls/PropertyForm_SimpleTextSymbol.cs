using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Symbology;
using gView.Framework.UI;

namespace gView.Framework.Symbology.UI
{
    internal partial class PropertyForm_SimpleTextSymbol : Form,IPropertyPageUI,gView.Framework.Symbology.UI.IPropertyPanel
    {
        private ITextSymbol _symbol = null;
        public PropertyForm_SimpleTextSymbol()
        {
            InitializeComponent();
        }

        #region IPropertyPageUI Members

        public event PropertyChangedEvent PropertyChanged;

        #endregion

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (PropertyChanged != null) PropertyChanged(_symbol);
        }

        #region IPropertyPanel Member

        public object PropertyPanel(ISymbol symbol)
        {
            if (!(symbol is ITextSymbol)) return null;

            _symbol = (ITextSymbol)symbol;
            propertyGrid.SelectedObject = new CustomClass(_symbol);

            return panelTextSymbol;
        }

        #endregion
    }
}