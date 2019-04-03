using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using System.Xml;
using gView.Framework.FDB;
using gView.Framework.Data;
using System.Linq;

namespace gView.Framework.UI.Controls
{
    public partial class AdditionalFieldsControl : UserControl
    {
        public AdditionalFieldsControl()
        {
            InitializeComponent();

            PlugInManager compMan = new PlugInManager();
            var autoFields = compMan.GetPlugins(Plugins.Type.IAutoField);

            if (autoFields != null)
            {
                tablePanel.RowCount = autoFields.Count();
                tablePanel.Height = autoFields.Count() * 25;
                tablePanel.Resize += new EventHandler(tablePanel_Resize);

                int row = 0;
                foreach (var compType in autoFields)
                {
                    IAutoField aField = compMan.CreateInstance<IAutoField>(compType);
                    if (aField == null) continue;

                    TextBox box = new TextBox();
                    AutoFieldCheckBox checkBox = new AutoFieldCheckBox(aField, box);

                    box.Text = aField.AutoFieldPrimayName;

                    tablePanel.Controls.Add(checkBox, 0, row);
                    tablePanel.Controls.Add(box, 1, row++);
                }
            }
        }

        void tablePanel_Resize(object sender, EventArgs e)
        {
            foreach (Control control in tablePanel.Controls)
            {
                if(control is CheckBox)
                    control.Width = tablePanel.Width - 120;
                if (control is TextBox)
                    control.Width = 120;
            }
        }

        public Fields AdditionalFields
        {
            get
            {
                Fields fields=new Fields();
                foreach (Control control in tablePanel.Controls)
                {
                    if (control is AutoFieldCheckBox && ((AutoFieldCheckBox)control).Checked)
                        fields.Add(((AutoFieldCheckBox)control).AutoField);
                }

                return fields;
            }
        }
    }

    internal class AutoFieldCheckBox : CheckBox
    {
        private IAutoField _field;
        private TextBox _nameBox;

        public AutoFieldCheckBox(IAutoField field, TextBox nameBox)
        {
            if (field == null || nameBox == null) return;

            _field = field;
            _nameBox = nameBox;

            this.Text = _field.AutoFieldName;

            if (_field.AutoFieldDescription != String.Empty)
                this.Text += (": " + _field.AutoFieldDescription);

            _nameBox.TextChanged += new EventHandler(nameBox_TextChanged);
        }

        void nameBox_TextChanged(object sender, EventArgs e)
        {
            if (_field is Field)
                ((Field)_field).name = _nameBox.Text;
        }

        public IAutoField AutoField
        {
            get { return _field; }
        }
    }
}
