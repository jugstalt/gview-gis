using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using gView.Framework.UI;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormCommandParameters : Form
    {
        private XmlNode _commandDef;
        private List<Control> _controls;
        private string _command = "";
        private IExplorerObject _exObject;

        public FormCommandParameters(IExplorerObject exObject, XmlNode commandDef)
        {
            InitializeComponent();

            _exObject = exObject;
            if (_exObject == null) return;
            _commandDef = commandDef;
            if (_commandDef == null) return;

            MakeGUI();
        }

        private void MakeGUI()
        {
            if (_commandDef == null) return;

            try
            {
                this.Text = _commandDef.Attributes["name"].Value;

                XmlNodeList parameters = _commandDef.SelectNodes("parameters/parameter");
                tablePanel.RowCount = parameters.Count;
                tablePanel.ColumnCount = 3;

                _controls = new List<Control>();
                int row = 0;
                foreach (XmlNode parameter in parameters)
                {
                    Label label = new Label();
                    label.Text = parameter.Attributes["name"].Value;
                    label.Size = new Size(100, 20);
                    label.Dock = DockStyle.Top;
                    label.TextAlign = ContentAlignment.MiddleRight;
                    if (label.Text.IndexOf(":") == -1) label.Text += " :";
                    tablePanel.Controls.Add(label, 0, row);

                    TextBox tb;
                    Button btn;
                    int val;
                    string defParameter = "";
                    if (_exObject is IExplorerObjectCommandParameters)
                    {
                        Dictionary<string, string> p = ((IExplorerObjectCommandParameters)_exObject).Parameters;
                        p.TryGetValue(parameter.Attributes["name"].Value, out defParameter);
                    }
                    switch (parameter.Attributes["type"].Value.ToLower())
                    {
                        case "file":
                            tb = new TextBox();
                            tb.Dock = DockStyle.Top;
                            if (defParameter != "") tb.Text = defParameter;
                            tablePanel.Controls.Add(tb, 1, row);
                            btn = new Button();
                            btn.Text = "...";
                            btn.Name = row.ToString();
                            btn.Click += new EventHandler(Button_Click);
                            btn.Size = new Size(btn.Width, 20);
                            btn.Dock = DockStyle.Top;
                            tablePanel.Controls.Add(btn, 2, row);
                            _controls.Add(tb);
                            break;
                        
                        case "string":
                            tb = new TextBox();
                            tb.Dock = DockStyle.Top;
                            if (defParameter != "") tb.Text = defParameter;
                            tablePanel.Controls.Add(tb, 1, row);
                            _controls.Add(tb);
                            break;

                        case "number":
                            NumericUpDown updown = new NumericUpDown();
                            updown.Dock = DockStyle.Top;
                            if (parameter.Attributes["value"] != null && int.TryParse(parameter.Attributes["value"].Value, out val))
                            {
                                updown.Value = val;
                            }
                            tablePanel.Controls.Add(updown, 1, row);
                            _controls.Add(updown);
                            break;

                        case "option":
                            ComboBox cmb = new ComboBox();
                            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
                            cmb.Dock = DockStyle.Top;
                            foreach (string v in parameter.Attributes["values"].Value.Split('|'))
                            {
                                cmb.Items.Add(v);
                            }
                            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
                            tablePanel.Controls.Add(cmb, 1, row);
                            _controls.Add(cmb);
                            break;
                    }
                    row++;
                }
                this.Height = panel1.Height + panel2.Height + 35 + row * 28;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void Button_Click(object sender, EventArgs e)
        {
            if (_commandDef == null) return;
            try
            {
                int row = 0;
                if (!int.TryParse(((Button)sender).Name, out row)) return;

                XmlNode parameter = _commandDef.SelectNodes("parameters/parameter")[row];

                switch (parameter.Attributes["dialog"].Value.ToLower())
                {
                    case "open":
                        OpenFileDialog open = new OpenFileDialog();
                        open.Title = parameter.Attributes["name"].Value;
                        if (parameter.Attributes["filter"] != null) open.Filter = parameter.Attributes["filter"].Value;

                        if (_controls[row].Text.Trim() != "")
                        {
                            open.FileName = ((_controls[row].Text.Trim().Replace("/", @"\").LastIndexOf(@"\") == _controls[row].Text.Trim().Length - 1) ? _controls[row].Text + "Filename" : _controls[row].Text);
                        }
                        if (open.ShowDialog() == DialogResult.OK)
                        {
                            _controls[row].Text = open.FileName;
                        }
                        break;
                    case "save":
                        SaveFileDialog save = new SaveFileDialog();
                        save.Title = parameter.Attributes["name"].Value;
                        if (parameter.Attributes["filter"] != null) save.Filter = parameter.Attributes["filter"].Value;

                        if (_controls[row].Text.Trim() != "")
                        {
                            save.FileName = ((_controls[row].Text.Trim().Replace("/", @"\").LastIndexOf(@"\") == _controls[row].Text.Trim().Length - 1) ? _controls[row].Text + "Filename" : _controls[row].Text);
                        }
                        if (save.ShowDialog() == DialogResult.OK)
                        {
                            _controls[row].Text = save.FileName;
                        }
                        break;
                }
            }
            catch { }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_commandDef == null) return;

            StringBuilder sb = new StringBuilder();
            XmlNodeList exes = _commandDef.SelectNodes("exe[@call]");
            foreach (XmlNode exe in exes)
            {
                string call = exe.Attributes["call"].Value;
                for (int i = 0; i < _controls.Count;i++ )
                {
                    Control ctrl = _controls[i];
                    string txt = ctrl.Text;
                    if (ctrl is NumericUpDown) txt = txt.Replace(",", ".");

                    call = call.Replace("{" + i + "}", ((txt.IndexOf(" ") == -1) ? txt : "\"" + txt + "\""));
                }
                if (sb.Length > 0) sb.Append("\r\n");
                sb.Append(call);
            }

            _command = sb.ToString();
        }

        public string Command
        {
            get { return _command; }
        }
    }
}