using gView.Framework.Data;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace gView.Plugins.Editor.Controls
{
    internal partial class AttributeControl : UserControl, IErrorMessage
    {
        private Module _module = null;
        private IFeatureClass _fc = null;
        private string _errMsg = String.Empty;

        public AttributeControl()
        {
            InitializeComponent();
        }

        public Module Module
        {
            get { return _module; }
            set { _module = value; }
        }

        private delegate void RefreshGUICallback();
        async public void RefreshGUI()
        {
            if (this.InvokeRequired)
            {
                RefreshGUICallback d = new RefreshGUICallback(RefreshGUI);
                this.BeginInvoke(d);
                return;
            }
            if (_module == null || _module.FeatureClass == null)
            {
                //panelFieldNames.Controls.Clear();
                //panelValues.Controls.Clear();
                panelValues.Enabled = false;
                return;
            }

            if (_fc != _module.FeatureClass)
            {
                panelFieldNames.Controls.Clear();
                panelValues.Controls.Clear();
                _fc = _module.FeatureClass;

                IFeatureLayer layer = _module.GetFeatureClassLayer(_fc);
                if (layer == null || layer.Fields == null)
                {
                    return;
                }

                List<IField> fields = new List<IField>();

                foreach (IField f in layer.Fields.ToEnumerable())
                {
                    fields.Add(f);
                }

                int height = 20;
                foreach (IField field in ListOperations<IField>.Swap(fields))
                {
                    if (!field.IsEditable)
                    {
                        continue;
                    }

                    if (field.type == FieldType.Shape ||
                        field.type == FieldType.binary)
                    {
                        continue;
                    }

                    #region Label
                    Panel p1 = new Panel();
                    p1.Height = height;
                    p1.Dock = DockStyle.Top;

                    TextBox tbLabel = new TextBox();
                    tbLabel.Text = field.aliasname + ":";
                    tbLabel.TextAlign = HorizontalAlignment.Right;
                    tbLabel.ReadOnly = true;
                    tbLabel.BackColor = Color.White;
                    p1.Controls.Add(tbLabel);
                    tbLabel.Dock = DockStyle.Fill;

                    //Label label = new Label();
                    //label.Text = field.name + ":";
                    //label.TextAlign = ContentAlignment.MiddleRight;
                    //p1.Controls.Add(label);
                    //label.Dock = DockStyle.Fill;

                    panelFieldNames.Controls.Add(p1);
                    #endregion

                    #region ValueControl
                    Panel p2 = new Panel();
                    p2.Height = height;
                    p2.Dock = DockStyle.Top;

                    Control ctrl = null;
                    if (field.Domain is IValuesFieldDomain)
                    {
                        ctrl = new ComboControl(_module.Feature, field, await ((IValuesFieldDomain)field.Domain).ValuesAsync());
                    }
                    else
                    {
                        switch (field.type)
                        {
                            case FieldType.smallinteger:
                            case FieldType.integer:
                            case FieldType.biginteger:
                                ctrl = new MaskedTextBox(_module.Feature, field);
                                ((MaskedTextBox)ctrl).DigitOnly = true;
                                break;
                            case FieldType.Float:
                            case FieldType.Double:
                                //ctrl = new NumberControl(_module.Feature, field);
                                ctrl = new MaskedTextBox(_module.Feature, field);
                                ((MaskedTextBox)ctrl).DecimalOnly = true;
                                break;
                            default:
                                ctrl = new TextControl(_module.Feature, field);
                                break;
                        }
                    }
                    p2.Controls.Add(ctrl);
                    ctrl.Dock = DockStyle.Fill;
                    panelValues.Controls.Add(p2);
                    #endregion
                }
                panel1.Height = panelFieldNames.Controls.Count * height + 1;
            }
            else
            {
                SetFeatureValues(panelValues);
            }

            panelValues.Enabled = panelFieldNames.Enabled = (_module.Feature != null);
        }
        public bool CommitValues()
        {
            _errMsg = String.Empty;
            return CommitValues(panelValues);
        }
        private bool CommitValues(Control parent)
        {
            if (parent == null)
            {
                return false;
            }

            foreach (Control child in parent.Controls)
            {
                if (child == null)
                {
                    continue;
                }

                if (child is IFieldControl)
                {
                    if (!((IFieldControl)child).Commit())
                    {
                        _errMsg = ((IFieldControl)child).LastErrorMessage;
                        child.Focus();
                        return false;
                    }
                }
                else
                    if (!CommitValues(child))
                {
                    return false;
                }
            }

            return true;
        }

        private void SetFeatureValues(Control parent)
        {
            if (parent == null)
            {
                return;
            }

            foreach (Control child in parent.Controls)
            {
                if (child == null)
                {
                    continue;
                }

                if (child is IFieldControl)
                {
                    ((IFieldControl)child).Feature = _module.Feature;
                }
                else
                {
                    SetFeatureValues(child);
                }
            }
        }

        #region ControlClasses
        private interface IFieldControl : IErrorMessage
        {
            IFeature Feature { get; set; }
            bool Commit();
        }

        private class TextControl : TextBox, IFieldControl
        {
            protected IFeature _feature = null;
            protected IField _field;
            private String _origValue = null, _errMsg = String.Empty;

            public TextControl(IFeature feature, IField field)
            {
                _field = field;

                this.Feature = feature;
                AddEventHandlers();

                SetBackColor();
            }

            private void AddEventHandlers()
            {
                base.TextChanged += new EventHandler(TextControl_TextChanged);
            }

            private void RemoveEventHandlers()
            {
                base.TextChanged -= new EventHandler(TextControl_TextChanged);
            }
            void TextControl_TextChanged(object sender, EventArgs e)
            {
                Commit();
            }

            #region IFieldControl Member

            public IFeature Feature
            {
                get
                {
                    return _feature;
                }
                set
                {
                    if (_feature != value)
                    {
                        //base.BackColor = Color.White;
                        SetBackColor();

                        _feature = value;

                        if (_feature != null)
                        {
                            _origValue = (_feature[_field.name] != null) ? _feature[_field.name].ToString() : String.Empty;
                            base.Text = _origValue;
                            base.Enabled = (_field.type != FieldType.ID && _field.type != FieldType.replicationID);
                        }
                        else
                        {
                            _origValue = null;
                            base.Text = String.Empty;
                            base.Enabled = false;
                        }
                    }
                }
            }

            virtual public bool Commit()
            {
                _errMsg = String.Empty;

                if (_feature != null)
                {
                    Module.SetValueOrAppendFieldValueIfNotExist(Feature, _field.name, this.Text);

                    SetBackColor();

                    if (_field.IsRequired && String.IsNullOrEmpty(this.Text))
                    {
                        _errMsg = "Value '" + _field.name + "' is required...";
                        return false;
                    }
                }
                else
                {
                    base.Enabled = false;
                }

                return true;
            }

            private void SetBackColor()
            {
                if (_feature != null)
                {
                    if (_origValue != null)
                    {
                        if (_origValue.Equals(this.Text))
                        {
                            base.BackColor = Color.White;
                        }
                        else
                        {
                            base.BackColor = Color.Yellow;
                        }
                    }
                    else
                    {
                        if (_feature[_field.name] == null)
                        {
                            base.BackColor = Color.White;
                        }
                        else
                        {
                            base.BackColor = Color.Yellow;
                        }
                    }

                    if (_field.IsRequired && String.IsNullOrEmpty(this.Text))
                    {
                        base.BackColor = Color.Red;
                    }
                }
                else
                {
                    if (_field != null && _field.IsRequired && String.IsNullOrEmpty(this.Text))
                    {
                        base.BackColor = Color.Red;
                    }
                    else
                    {
                        base.BackColor = Color.White;
                    }
                }
            }

            #endregion

            #region IErrorMessage Member

            public string LastErrorMessage
            {
                get { return _errMsg; }
                set { _errMsg = value; }
            }

            #endregion
        }

        private class NumberControl : NumericUpDown, IFieldControl
        {
            private IFeature _feature = null;
            private IField _field;
            private object _origValue = null;
            private string _errMsg = String.Empty;

            public NumberControl(IFeature feature, IField field)
            {
                _field = field;

                switch (field.type)
                {
                    case FieldType.smallinteger:
                        base.Minimum = short.MinValue;
                        base.Maximum = short.MaxValue;
                        base.DecimalPlaces = 0;
                        break;
                    case FieldType.integer:
                        base.Minimum = int.MinValue;
                        base.Maximum = int.MaxValue;
                        base.DecimalPlaces = 0;
                        break;
                    case FieldType.biginteger:
                        base.Minimum = long.MinValue;
                        base.Maximum = long.MaxValue;
                        base.DecimalPlaces = 0;
                        break;
                    case FieldType.Float:
                        base.Minimum = decimal.MinValue;
                        base.Maximum = decimal.MaxValue;
                        base.DecimalPlaces = 7;
                        break;
                    case FieldType.Double:
                        base.Minimum = decimal.MinValue;
                        base.Maximum = decimal.MaxValue;
                        base.DecimalPlaces = 16;
                        break;
                }

                this.Feature = feature;
                base.ValueChanged += new EventHandler(NumberControl_ValueChanged);
                base.Leave += new EventHandler(NumberControl_ValueChanged);
                base.KeyPress += new KeyPressEventHandler(NumberControl_KeyPress);
            }

            void NumberControl_KeyPress(object sender, KeyPressEventArgs e)
            {
                NumberControl_ValueChanged(sender, new EventArgs());
            }

            void NumberControl_ValueChanged(object sender, EventArgs e)
            {
                Commit();
            }

            #region IFieldControl Member

            public IFeature Feature
            {
                get
                {
                    return _feature;
                }
                set
                {
                    if (_feature != value)
                    {
                        base.BackColor = Color.White;

                        _feature = value;

                        if (_feature != null)
                        {
                            _origValue = _feature[_field.name];
                            base.Value = (_origValue != null &&
                                          _origValue != DBNull.Value) ?
                                          Convert.ToDecimal(_origValue) : (decimal)0.0;

                            base.Enabled = true;
                        }
                        else
                        {
                            _origValue = null;
                            base.Value = (decimal)0.0;
                            base.Enabled = false;
                        }
                    }
                }
            }

            public bool Commit()
            {
                if (_feature == null)
                {
                    return false;
                }

                switch (_field.type)
                {
                    case FieldType.smallinteger:
                        Module.SetValueOrAppendFieldValueIfNotExist(
                            _feature, _field.name, (short)base.Value);
                        break;
                    case FieldType.integer:
                        Module.SetValueOrAppendFieldValueIfNotExist(
                            _feature, _field.name, (int)base.Value);
                        break;
                    case FieldType.biginteger:
                        Module.SetValueOrAppendFieldValueIfNotExist(
                            _feature, _field.name, (long)base.Value);
                        break;
                    case FieldType.Float:
                        Module.SetValueOrAppendFieldValueIfNotExist(
                            _feature, _field.name, (float)base.Value);
                        break;
                    case FieldType.Double:
                        Module.SetValueOrAppendFieldValueIfNotExist(
                            _feature, _field.name, (double)base.Value);
                        break;
                }

                if (_origValue != null)
                {
                    if (CompareValues(_origValue, _feature[_field.name]))
                    {
                        base.BackColor = Color.White;
                    }
                    else
                    {
                        base.BackColor = Color.Yellow;
                    }
                }
                else
                {
                    if (_feature[_field.name] == null)
                    {
                        base.BackColor = Color.White;
                    }
                    else
                    {
                        base.BackColor = Color.Yellow;
                    }
                }

                return true;
            }
            #endregion

            private bool CompareValues(object var1, object var2)
            {
                if (var1 == null && var2 == null)
                {
                    return true;
                }

                if (var1 != null && var1.Equals(var2))
                {
                    return true;
                }

                try
                {
                    double v1 = Convert.ToDouble(var1);
                    double v2 = Convert.ToDouble(var2);

                    return v1 == v2;
                }
                catch { }
                return false;
            }

            #region IErrorMessage Member

            public string LastErrorMessage
            {
                get { return _errMsg; }
                set { _errMsg = value; }
            }

            #endregion
        }

        private class ComboControl : ComboBox, IFieldControl
        {
            private IFeature _feature = null;
            private IField _field;
            private object _origValue = null;
            private string _errMsg = String.Empty;

            public ComboControl(IFeature feature, IField field, object[] values)
            {
                this.DropDownStyle = ComboBoxStyle.DropDownList;

                if (values == null)
                {
                    return;
                }

                foreach (object v in values)
                {
                    if (v == null)
                    {
                        continue;
                    }

                    this.Items.Add(v);
                }
                _field = field;
                this.Feature = feature;

                base.DrawMode = DrawMode.OwnerDrawFixed;
                base.DrawItem += new DrawItemEventHandler(ComboControl_DrawItem);
                base.SelectedIndexChanged += new EventHandler(ComboControl_SelectedIndexChanged);

            }

            void ComboControl_DrawItem(object sender, DrawItemEventArgs e)
            {
                if (e.Index == -1)
                {
                    return;
                }

                object item = base.Items[e.Index];
                if (item == null)
                {
                    return;
                }

                RectangleF rect = new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

                if (Bit.Has(e.State, DrawItemState.ComboBoxEdit) &&
                    _field != null && _field.IsRequired &&
                    (base.SelectedItem == null || String.IsNullOrEmpty(base.SelectedItem.ToString())))
                {
                    e.Graphics.FillRectangle(Brushes.Red, rect);
                }
                else
                {
                    e.Graphics.FillRectangle(
                        CompareValues(item, _origValue) ? Brushes.White : Brushes.Yellow,
                        rect);
                }
                e.Graphics.DrawString(
                             item.ToString(),
                             e.Font,
                             Brushes.Black,
                             rect);
                e.DrawFocusRectangle();

                //switch (e.State)
                //{   
                //    case DrawItemState.Selected:

                //        e.Graphics.DrawString(
                //            item.ToString(),
                //            e.Font,
                //            Brushes.Blue,
                //            rect);
                //        break;
                //    default:

                //        break;
                //}
            }

            void ComboControl_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (CompareValues(base.SelectedItem, _origValue))
                {
                    base.BackColor = Color.White;
                }
                else
                {
                    base.BackColor = Color.Yellow;
                }
            }

            private bool CompareValues(object var1, object var2)
            {
                if (var1 == null && var2 == null)
                {
                    return true;
                }

                if (var1 != null && var2 != null && var1.ToString() == var2.ToString())
                {
                    return true;
                }

                if (var1 != null)
                {
                    return var1.Equals(var2);
                }

                return false;
            }
            #region IFieldControl Member

            public IFeature Feature
            {
                get
                {
                    return _feature;
                }
                set
                {
                    if (_feature != value)
                    {
                        base.BackColor = Color.White;

                        _feature = value;

                        if (_feature != null)
                        {
                            _origValue = _feature[_field.name];
                            if (_origValue == null)
                            {
                                base.SelectedIndex = -1;
                            }
                            else
                            {
                                base.SelectedItem = _origValue.ToString();
                            }

                            base.Enabled = true;
                        }
                        else
                        {
                            _origValue = null;
                            base.SelectedIndex = -1;
                            base.Enabled = false;
                        }
                    }
                }
            }

            public bool Commit()
            {
                _errMsg = String.Empty;

                if (_field.IsRequired && (this.SelectedItem == null || String.IsNullOrEmpty(this.SelectedItem.ToString())))
                {
                    _errMsg = "Value '" + _field.name + "' is required...";
                    return false;
                }
                if (this.SelectedItem != null && _feature != null && _field != null)
                {
                    Module.SetValueOrAppendFieldValueIfNotExist(
                        _feature, _field.name, this.SelectedItem);
                }
                return true;
            }

            #endregion

            #region IErrorMessage Member

            public string LastErrorMessage
            {
                get { return _errMsg; }
                set { _errMsg = value; }
            }

            #endregion
        }

        private class MaskedTextBox : TextControl
        {
            private char _decimalSeperator = '.';
            private bool m_dateOnly;
            private bool m_phoneOnly;
            private bool m_IPAddrOnly;
            private bool m_ssn;
            private bool m_decimalOnly;
            private bool m_digitOnly;

            private int digitPos = 0;

            private int DelimitNumber = 0;
            private System.Windows.Forms.ErrorProvider errorProvider1;

            private System.ComponentModel.Container components = null;

            public bool DecimalOnly
            {
                get { return m_decimalOnly; }
                set
                {
                    m_decimalOnly = value;
                    if (value)
                    {
                        try
                        {
                            _decimalSeperator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
                        }
                        catch { _decimalSeperator = '.'; }
                    }
                }
            }
            public bool PhoneWithAreaCode
            {
                get { return m_phoneOnly; }
                set { m_phoneOnly = value; }
            }
            public bool DateOnly
            {
                get { return m_dateOnly; }
                set { m_dateOnly = value; }
            }
            public bool SSNOnly
            {
                get { return m_ssn; }
                set { m_ssn = value; }
            }
            public bool IPAddrOnly
            {
                get { return m_IPAddrOnly; }
                set { m_IPAddrOnly = value; }
            }
            public bool DigitOnly
            {
                get { return m_digitOnly; }
                set { m_digitOnly = value; }
            }

            public MaskedTextBox(IFeature feature, IField field)
                : base(feature, field)
            {

                // This call is required by the Windows.Forms Form Designer.
                InitializeComponent();

                // TODO: Add any initialization after the InitForm call

            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (components != null)
                    {
                        components.Dispose();
                    }
                }
                base.Dispose(disposing);
            }

            #region Component Designer generated code
            /// <summary>
            /// Required method for Designer support - do not modify 
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
                this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
                // 
                // errorProvider1
                // 
                this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
                // 
                // MaskedTextBox
                // 
                this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);

            }
            #endregion
            private void OnKeyPress(object sender, KeyPressEventArgs e)
            {
                MaskedTextBox sd = (MaskedTextBox)sender;
                if (sd.m_IPAddrOnly)
                {
                    sd.MaskIpAddr(e);
                }

                if (sd.m_digitOnly)
                {
                    sd.MaskDigit(e);
                }

                if (sd.m_ssn)
                {
                    sd.MaskPhoneSSN(e, 3, 2);
                }

                if (sd.m_phoneOnly)
                {
                    sd.MaskPhoneSSN(e, 3, 3);
                }

                if (sd.m_dateOnly)
                {
                    sd.MaskDate(e);
                }

                if (sd.m_decimalOnly)
                {
                    sd.MaskDecimal(e);
                }
            }
            private void MaskDigit(KeyPressEventArgs e)
            {
                if (Char.IsDigit(e.KeyChar) || e.KeyChar == 8 || e.KeyChar == '-' || e.KeyChar == '+')
                {
                    string text = InsertKeyToText(e);
                    try
                    {
                        switch (_field.type)
                        {
                            case FieldType.smallinteger:
                                short.Parse(text);
                                break;
                            case FieldType.integer:
                                int.Parse(text);
                                break;
                            case FieldType.biginteger:
                                long.Parse(text);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Handled = true;
                        errorProvider1.SetError(this, ex.Message);
                        return;
                    }
                    errorProvider1.SetError(this, "");
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                    errorProvider1.SetError(this, "Only valid for Digit");
                }
            }
            private void MaskDecimal(KeyPressEventArgs e)
            {
                if (Char.IsDigit(e.KeyChar) || e.KeyChar == _decimalSeperator || e.KeyChar == 8 || e.KeyChar == '-' || e.KeyChar == '+')
                {
                    string text = InsertKeyToText(e);
                    try
                    {
                        switch (_field.type)
                        {
                            case FieldType.Float:
                                float.Parse(text);
                                break;
                            case FieldType.Double:
                                double.Parse(text);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Handled = true;
                        errorProvider1.SetError(this, ex.Message);
                        return;
                    }

                    e.Handled = false;
                    errorProvider1.SetError(this, "");
                }
                else
                {
                    e.Handled = true;
                    errorProvider1.SetError(this, "Only valid for Digit and dot");
                }
            }
            private void MaskDate(KeyPressEventArgs e)
            {
                int len = this.Text.Length;
                int indx = this.Text.LastIndexOf("/");
                // if test is highlighted reset vars
                if (this.SelectedText == this.Text)
                {
                    indx = -1;
                    digitPos = 0;
                    DelimitNumber = 0;
                    this.Text = null;
                }
                if (Char.IsDigit(e.KeyChar) || e.KeyChar == '/' || e.KeyChar == 8)
                {
                    string tmp = this.Text;
                    if (e.KeyChar != 8)
                    {
                        if (e.KeyChar != '/')
                        {
                            if (indx > 0)
                            {
                                digitPos = len - indx;
                            }
                            else
                            {
                                digitPos++;
                            }

                            if (digitPos == 3 && DelimitNumber < 2)
                            {
                                if (e.KeyChar != '/')
                                {
                                    DelimitNumber++;
                                    this.AppendText("/");
                                }
                            }

                            errorProvider1.SetError(this, "");
                            if ((digitPos == 2 || (Int32.Parse(e.KeyChar.ToString()) > 1 && DelimitNumber == 0)))
                            {
                                string tmp2;
                                if (indx == -1)
                                {
                                    tmp2 = e.KeyChar.ToString();
                                }
                                else
                                {
                                    tmp2 = this.Text.Substring(indx + 1) + e.KeyChar.ToString();
                                }

                                if (DelimitNumber < 2)
                                {
                                    if (digitPos == 1)
                                    {
                                        this.AppendText("0");
                                    }

                                    this.AppendText(e.KeyChar.ToString());
                                    if (indx < 0)
                                    {
                                        if (Int32.Parse(this.Text) > 12) // check validation
                                        {
                                            string str;
                                            str = this.Text.Insert(0, "0");
                                            if (Int32.Parse(this.Text) > 13)
                                            {
                                                this.Text = str.Insert(2, "/0");
                                                DelimitNumber++;
                                                this.AppendText("/");
                                            }
                                            else
                                            {
                                                this.Text = str.Insert(2, "/");
                                                this.AppendText("");
                                            }
                                            DelimitNumber++;
                                        }
                                        else
                                        {
                                            this.AppendText("/");
                                            DelimitNumber++;
                                        }
                                        e.Handled = true;
                                    }
                                    else
                                    {
                                        if (DelimitNumber == 1)
                                        {
                                            int m = Int32.Parse(this.Text.Substring(0, indx));
                                            if (!CheckDayOfMonth(m, Int32.Parse(tmp2)))
                                            {
                                                errorProvider1.SetError(this, "Make sure this month have the day");
                                            }
                                            else
                                            {
                                                this.AppendText("/");
                                                DelimitNumber++;
                                                e.Handled = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (digitPos == 1 && Int32.Parse(e.KeyChar.ToString()) > 3 && DelimitNumber < 2)
                            {
                                if (digitPos == 1)
                                {
                                    this.AppendText("0");
                                }

                                this.AppendText(e.KeyChar.ToString());
                                this.AppendText("/");
                                DelimitNumber++;
                                e.Handled = true;
                            }
                            else
                            {
                                if (digitPos == 1 && DelimitNumber == 2 && e.KeyChar > '2')
                                {
                                    errorProvider1.SetError(this, "The year should start with 1 or 2");
                                }
                            }
                        }
                        else
                        {
                            DelimitNumber++;
                            string tmp3;
                            if (indx == -1)
                            {
                                tmp3 = this.Text.Substring(indx + 1);
                            }
                            else
                            {
                                tmp3 = this.Text;
                            }

                            if (digitPos == 1)
                            {
                                this.Text = tmp3.Insert(indx + 1, "0"); ;
                                this.AppendText("/");
                                e.Handled = true;
                            }
                        }
                    }
                    else
                    {
                        e.Handled = false;
                        if ((len - indx) == 1)
                        {
                            DelimitNumber--;
                            if (indx > -1)
                            {
                                digitPos = 2;
                            }
                            else
                            {
                                digitPos--;
                            }
                        }
                        else
                        {
                            if (indx > -1)
                            {
                                digitPos = len - indx - 1;
                            }
                            else
                            {
                                digitPos = len - 1;
                            }
                        }
                    }
                }
                else
                {
                    e.Handled = true;
                    errorProvider1.SetError(this, "Only valid for Digit and /");
                }
            }
            private void MaskPhoneSSN(KeyPressEventArgs e, int pos, int pos2)
            {
                int len = this.Text.Length;
                int indx = this.Text.LastIndexOf("-");
                // if test is highlighted reset vars
                if (this.SelectedText == this.Text)
                {
                    indx = -1;
                    digitPos = 0;
                    DelimitNumber = 0;
                }
                if (Char.IsDigit(e.KeyChar) || e.KeyChar == '-' || e.KeyChar == 8)
                { // only digit, Backspace and - are accepted
                    string tmp = this.Text;
                    if (e.KeyChar != 8)
                    {
                        errorProvider1.SetError(this, "");
                        if (e.KeyChar != '-')
                        {
                            if (indx > 0)
                            {
                                digitPos = len - indx;
                            }
                            else
                            {
                                digitPos++;
                            }
                        }
                        if (indx > -1 && digitPos == pos2 && DelimitNumber == 1)
                        {
                            if (e.KeyChar != '-')
                            {
                                this.AppendText(e.KeyChar.ToString());
                                this.AppendText("-");
                                e.Handled = true;
                                DelimitNumber++;
                            }
                        }
                        else if (digitPos == pos && DelimitNumber == 0)
                        {
                            if (e.KeyChar != '-')
                            {
                                this.AppendText(e.KeyChar.ToString());
                                this.AppendText("-");
                                e.Handled = true;
                                DelimitNumber++;
                            }
                        }
                    }
                    else
                    {
                        e.Handled = false;
                        if ((len - indx) == 1)
                        {
                            DelimitNumber--;
                            if ((indx) > -1)
                            {
                                digitPos = len - indx;
                            }
                            else
                            {
                                digitPos--;
                            }
                        }
                        else
                        {
                            if (indx > -1)
                            {
                                digitPos = len - indx - 1;
                            }
                            else
                            {
                                digitPos = len - 1;
                            }
                        }
                    }
                }
                else
                {
                    e.Handled = true;
                    errorProvider1.SetError(this, "Only valid for Digit and -");
                }
            }
            private void MaskIpAddr(KeyPressEventArgs e)
            {
                int len = this.Text.Length;
                int indx = this.Text.LastIndexOf(".");
                // if test is highlighted reset vars
                if (this.SelectedText == this.Text)
                {
                    indx = -1;
                    digitPos = 0;
                    DelimitNumber = 0;
                }
                if (Char.IsDigit(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == 8)
                { // only digit, Backspace and dot are accepted
                    string tmp = this.Text;
                    errorProvider1.SetError(this, "");
                    if (e.KeyChar != 8)
                    {
                        if (e.KeyChar != '.')
                        {
                            if (indx > 0)
                            {
                                digitPos = len - indx;
                            }
                            else
                            {
                                digitPos++;
                            }
                        }
                        if (digitPos == 3 && e.KeyChar != '.')
                        {
                            string tmp2 = this.Text.Substring(indx + 1) + e.KeyChar;
                            if (Int32.Parse(tmp2) > 255) // check validation
                            {
                                errorProvider1.SetError(this, "The number can't be bigger than 255");
                            }
                            else
                            {
                                if (DelimitNumber < 3)
                                {
                                    this.AppendText(e.KeyChar.ToString());
                                    this.AppendText(".");
                                    DelimitNumber++;
                                    e.Handled = true;
                                }
                            }
                        }
                        else if (digitPos == 4 && DelimitNumber < 3)
                        {
                            this.AppendText(".");
                        }
                    }
                    else
                    {
                        e.Handled = false;
                        if ((len - indx) == 1)
                        {
                            DelimitNumber--;
                            if (indx > -1)
                            {
                                digitPos = len - indx;
                            }
                            else
                            {
                                digitPos--;
                            }
                        }
                        else
                        {
                            if (indx > -1)
                            {
                                digitPos = len - indx - 1;
                            }
                            else
                            {
                                digitPos = len - 1;
                            }
                        }
                    }
                }
                else
                {
                    e.Handled = true;
                    errorProvider1.SetError(this, "Only valid for Digit abd dot");
                }
            }
            private bool CheckDayOfMonth(int mon, int day)
            {
                bool ret = true;
                if (day == 0)
                {
                    ret = false;
                }

                switch (mon)
                {
                    case 1:
                        if (day > 31)
                        {
                            ret = false;
                        }

                        break;
                    case 2:
                        if (day > 28)
                        {
                            ret = false;
                        }

                        break;
                    case 3:
                        if (day > 31)
                        {
                            ret = false;
                        }

                        break;
                    case 4:
                        if (day > 30)
                        {
                            ret = false;
                        }

                        break;
                    case 5:
                        if (day > 31)
                        {
                            ret = false;
                        }

                        break;
                    case 6:
                        if (day > 30)
                        {
                            ret = false;
                        }

                        break;
                    case 7:
                        if (day > 31)
                        {
                            ret = false;
                        }

                        break;
                    case 8:
                        if (day > 31)
                        {
                            ret = false;
                        }

                        break;
                    case 9:
                        if (day > 30)
                        {
                            ret = false;
                        }

                        break;
                    case 10:
                        if (day > 31)
                        {
                            ret = false;
                        }

                        break;
                    case 11:
                        if (day > 30)
                        {
                            ret = false;
                        }

                        break;
                    case 12:
                        if (day > 31)
                        {
                            ret = false;
                        }

                        break;
                    default:
                        ret = false;
                        break;
                }
                return ret;
            }

            private string InsertKeyToText(KeyPressEventArgs e)
            {
                string text = base.Text;

                if (base.SelectionStart == text.Length)
                {
                    if (e.KeyChar == 8) // BackSpace
                    {
                        return text.Substring(0, text.Length - 1);
                    }

                    return text + e.KeyChar;
                }
                else
                {
                    string t1 = text.Substring(0, base.SelectionStart);
                    string t2 = text.Substring(base.SelectionStart + base.SelectionLength,
                        text.Length - base.SelectionStart - base.SelectionLength);

                    if (e.KeyChar == 8)
                    {
                        if (base.SelectionLength == 0)
                        {
                            return t1 + t2;
                        }
                        else
                        {
                            return t1.Substring(0, t1.Length - 1) + t2;
                        }
                    }
                    return t1 + e.KeyChar + t2;
                }
            }

            public override bool Commit()
            {
                if (!base.Commit())
                {
                    return false;
                }

                if (String.IsNullOrEmpty(base.Text) && _field.IsRequired == false)
                {
                    Module.SetValueOrAppendFieldValueIfNotExist(Feature, _field.name, System.DBNull.Value);
                    return true;
                }
                try
                {
                    switch (_field.type)
                    {
                        case FieldType.smallinteger:
                            Module.SetValueOrAppendFieldValueIfNotExist(Feature, _field.name, Convert.ToInt16(base.Text));
                            break;
                        case FieldType.integer:
                            Module.SetValueOrAppendFieldValueIfNotExist(Feature, _field.name, Convert.ToInt32(base.Text));
                            break;
                        case FieldType.biginteger:
                            Module.SetValueOrAppendFieldValueIfNotExist(Feature, _field.name, Convert.ToInt64(base.Text));
                            break;
                        case FieldType.Float:
                            Module.SetValueOrAppendFieldValueIfNotExist(Feature, _field.name, Convert.ToSingle(base.Text));
                            break;
                        case FieldType.Double:
                            Module.SetValueOrAppendFieldValueIfNotExist(Feature, _field.name, Convert.ToDouble(base.Text));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }

                return true;
            }
        }

        //[Designer(typeof(NumberTextBox.NumberTextBoxDesigner))]
        private class NumberTextBox : TextControl
        {
            public const int WM_PASTE = 0x0302;
            public const int WM_CHAR = 0x0102;
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool MessageBeep(uint UType);

            public NumberTextBox(IFeature feature, IField field)
                : base(feature, field)
            {

            }

            protected override bool ProcessKeyEventArgs(ref Message m)
            {
                int nKey = m.WParam.ToInt32();
                if ((nKey >= (char)48 && nKey <= (char)57) ||
                nKey == (char)8 || nKey == (char)45 || nKey == (char)46)
                {
                    return base.ProcessKeyPreview(ref m);
                }
                else
                {
                    if (m.Msg == 256 && nKey == 46)
                    {
                        return base.ProcessKeyPreview(ref m);
                    }

                    if (m.Msg == 258)
                    {
                        MessageBeep(0);
                    }
                }
                return true;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_CHAR)
                {
                    int keyChar = m.WParam.ToInt32();
                    bool charOk = (keyChar > 47 && keyChar < 58) ||
                    keyChar == 8 ||
                    keyChar == 3 || keyChar == 22 || keyChar == 24;
                    if (!charOk)
                    {
                        return;
                    }
                }
                if (m.Msg == WM_PASTE)
                {
                    IDataObject iData = Clipboard.GetDataObject();
                    if (iData.GetDataPresent(DataFormats.Text))
                    {
                        string str;
                        str = (String)iData.GetData(DataFormats.Text);
                        if (!System.Text.RegularExpressions.Regex.IsMatch(str, @"^(\d{1,})$"))
                        {
                            return;
                        }
                    }
                }
                base.WndProc(ref m);
            }
        }

        private class NumberControl3 : TextControl
        {
            public NumberControl3(IFeature feature, IField field)
                : base(feature, field)
            {
                this.CausesValidation = true;
                this.Validating += new CancelEventHandler(NumberControl_Validating);
            }

            void NumberControl_Validating(object sender, CancelEventArgs e)
            {
                try
                {
                    switch (_field.type)
                    {
                        case FieldType.smallinteger:
                            short.Parse(this.Text);
                            break;
                        case FieldType.integer:
                            int.Parse(this.Text);
                            break;
                        case FieldType.biginteger:
                            long.Parse(this.Text);
                            break;
                        case FieldType.Float:
                            float.Parse(this.Text);
                            break;
                        case FieldType.Double:
                            double.Parse(this.Text);
                            break;
                    }
                }
                catch (Exception /*ex*/)
                {
                    e.Cancel = true;
                    //MessageBox.Show("Insert valid " + _field.type.ToString() + "-Number!");
                }
            }
        }

        #endregion

        #region IErrorMessage Member

        public string LastErrorMessage
        {
            get { return _errMsg; }
            set { _errMsg = value; }
        }

        #endregion
    }
}
