using gView.Framework.Data;
using gView.Framework.UI.Controls;
using System.Windows.Forms;

namespace gView.Framework.Carto.Rendering.UI
{
    internal partial class PropertyForm_QuantityRenderer_Dialog_InsertValue : Form
    {
        public PropertyForm_QuantityRenderer_Dialog_InsertValue(IField field)
        {
            InitializeComponent();

            if (field == null)
            {
                return;
            }

            switch (field.type)
            {
                case FieldType.ID:
                case FieldType.integer:
                    numFrom.DataType = numTo.DataType = NumericTextBox.NumericDataType.intType;
                    break;
                case FieldType.smallinteger:
                    numFrom.DataType = numTo.DataType = NumericTextBox.NumericDataType.shortType;
                    break;
                case FieldType.biginteger:
                    numFrom.DataType = numTo.DataType = NumericTextBox.NumericDataType.longType;
                    break;
                case FieldType.Float:
                    numFrom.DataType = numTo.DataType = NumericTextBox.NumericDataType.floatType;
                    break;
                case FieldType.Double:
                    numFrom.DataType = numTo.DataType = NumericTextBox.NumericDataType.doubleType;
                    break;
            }
        }

        public double MinValue
        {
            get { return numFrom.Double; }
        }
        public double MaxValue
        {
            get { return numTo.Double; }
        }
        public string Label
        {
            get { return txtLabel.Text; }
        }
    }
}