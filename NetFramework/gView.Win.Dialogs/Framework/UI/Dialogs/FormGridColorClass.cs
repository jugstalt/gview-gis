using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;

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
                    Color.Gray);
                colorClass.Legend = txtLabel.Text;

                return colorClass;
            }
            set
            {
                if (value == null) return;

                this.MinValue = value.MinValue;
                this.MaxValue = value.MaxValue;
                this.Label = value.Legend;
            }
        }
    }
}