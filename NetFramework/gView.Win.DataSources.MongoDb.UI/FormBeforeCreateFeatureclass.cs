using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.MongoDb.UI
{
    public partial class FormBeforeCreateFeatureclass : Form
    {
        public FormBeforeCreateFeatureclass()
        {
            InitializeComponent();
        }

        private void FormBeforeCreateFeatureclass_Load(object sender, EventArgs e)
        {
            double dpm = 96D / 0.0254;
            int y = 20, level = 0;
            foreach (var resolution in GeometryHelper.Resolutions)
            {
                var scale = resolution * dpm;
                string scaleText = String.Format("{0:n}", Math.Round(scale, 0));

                var checkBox = new CheckBox()
                {
                    Name = (level++).ToString(),
                    Text = $"1 : {scaleText}",
                    Left = 50,
                    Top = y,
                    Width = this.Width - 100
                };

                checkBox.Click += CheckBox_Click;

                y += 35;
                panelContent.Controls.Add(checkBox);
            }
        }

        private void CheckBox_Click(object sender, EventArgs e)
        {
            var level = int.Parse(((CheckBox)sender).Name);

            foreach (var control in panelContent.Controls)
            {
                if (control is CheckBox)
                {
                    var checkBox = (CheckBox)control;
                    var checkLevel = int.Parse(checkBox.Name);

                    if (checkLevel != level)
                    {
                        checkBox.Checked = checkLevel > level;
                    }
                }
            }
        }

        public int GeneralizeToLevel
        {
            get
            {
                foreach (var control in panelContent.Controls)
                {
                    if (control is CheckBox)
                    {
                        var checkBox = (CheckBox)control;
                        if (checkBox.Checked)
                        {
                            var checkLevel = int.Parse(checkBox.Name);
                            return checkLevel;
                        }
                    }
                }

                return -1;
            }
        }
    }
}
