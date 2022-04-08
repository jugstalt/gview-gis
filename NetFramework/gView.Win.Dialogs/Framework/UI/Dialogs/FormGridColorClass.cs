using gView.Framework.Data;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormGridColorClass : Form
    {
        public FormGridColorClass()
        {
            InitializeComponent();
        }

        public double MinValue
        {
            get { return (double)numMinValue.Value; }
            set { numMinValue.Value = (decimal)value; }
        }

        public double MaxValue
        {
            get { return (double)numMaxValue.Value; }
            set { numMaxValue.Value = (decimal)value; }
        }

        public string Label
        {
            get { return txtLabel.Text; }
            set { txtLabel.Text = value; }
        }

        public GridColorClass ColorClass
        {
            get
            {
                GridColorClass colorClass = new GridColorClass(
                    this.MinValue, this.MaxValue,
                    GraphicsEngine.ArgbColor.Gray);
                colorClass.Legend = txtLabel.Text;

                return colorClass;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                this.MinValue = value.MinValue;
                this.MaxValue = value.MaxValue;
                this.Label = value.Legend;
            }
        }
    }
}