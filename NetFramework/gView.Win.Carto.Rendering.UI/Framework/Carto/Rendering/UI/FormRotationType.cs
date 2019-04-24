using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Symbology;

namespace gView.Framework.Carto.Rendering.UI
{
	/// <summary>
	/// Zusammenfassung für FromRotationType.
	/// </summary>
	internal class FormRotationType : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel picGeographic;
		private System.Windows.Forms.Panel panelAritmetic;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbUnit;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		protected string str1,str2,str3,str4;
		protected int m_mode=0,m_unit=0;

		private System.Windows.Forms.Button btnOK;
        private ComboBox cmbFields;
        private Label label2;
		private System.Windows.Forms.Button btnCancel;
        private SymbolRotation _symbolRotation;

		public FormRotationType(SymbolRotation symbolRotation,ITableClass tableClass)
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();

            _symbolRotation = symbolRotation;
            string fieldname="";

            if (_symbolRotation != null)
            {
                fieldname = _symbolRotation.RotationFieldName;
                switch (_symbolRotation.RotationType)
                {
                    case RotationType.geographic:
                        radioButton1.Checked = true;
                        break;
                    case RotationType.aritmetic:
                        radioButton2.Checked = true;
                        break;
                }
                switch (_symbolRotation.RotationUnit)
                {
                    case RotationUnit.rad:
                        cmbUnit.SelectedIndex = 0;
                        break;
                    case RotationUnit.deg:
                        cmbUnit.SelectedIndex = 1;
                        break;
                    case RotationUnit.gon:
                        cmbUnit.SelectedIndex = 2;
                        break;
                }
            }
            if (tableClass != null)
            {
                cmbFields.Items.Add(new FieldItem(null));
                foreach (IField field in tableClass.Fields.ToEnumerable())
                {
                    if (field.type == FieldType.Double ||
                        field.type == FieldType.Float ||
                        field.type == FieldType.integer ||
                        field.type == FieldType.smallinteger ||
                        field.type == FieldType.biginteger)
                    {
                        FieldItem item = new FieldItem(field);
                        cmbFields.Items.Add(item);
                        if (field.name == fieldname) cmbFields.SelectedItem = item;
                    }
                }

                if (cmbFields.SelectedItem == null)
                    cmbFields.SelectedItem = cmbFields.Items[0];
            }
            else
            {
                cmbFields.Visible = false;
            }
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRotationType));
            this.picGeographic = new System.Windows.Forms.Panel();
            this.panelAritmetic = new System.Windows.Forms.Panel();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbUnit = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cmbFields = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // picGeographic
            // 
            this.picGeographic.AccessibleDescription = null;
            this.picGeographic.AccessibleName = null;
            resources.ApplyResources(this.picGeographic, "picGeographic");
            this.picGeographic.BackgroundImage = null;
            this.picGeographic.Font = null;
            this.picGeographic.Name = "picGeographic";
            this.picGeographic.Paint += new System.Windows.Forms.PaintEventHandler(this.picGeographic_Paint);
            // 
            // panelAritmetic
            // 
            this.panelAritmetic.AccessibleDescription = null;
            this.panelAritmetic.AccessibleName = null;
            resources.ApplyResources(this.panelAritmetic, "panelAritmetic");
            this.panelAritmetic.BackgroundImage = null;
            this.panelAritmetic.Font = null;
            this.panelAritmetic.Name = "panelAritmetic";
            this.panelAritmetic.Paint += new System.Windows.Forms.PaintEventHandler(this.panelAritmetic_Paint);
            // 
            // radioButton1
            // 
            this.radioButton1.AccessibleDescription = null;
            this.radioButton1.AccessibleName = null;
            resources.ApplyResources(this.radioButton1, "radioButton1");
            this.radioButton1.BackgroundImage = null;
            this.radioButton1.Font = null;
            this.radioButton1.Name = "radioButton1";
            // 
            // radioButton2
            // 
            this.radioButton2.AccessibleDescription = null;
            this.radioButton2.AccessibleName = null;
            resources.ApplyResources(this.radioButton2, "radioButton2");
            this.radioButton2.BackgroundImage = null;
            this.radioButton2.Font = null;
            this.radioButton2.Name = "radioButton2";
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // cmbUnit
            // 
            this.cmbUnit.AccessibleDescription = null;
            this.cmbUnit.AccessibleName = null;
            resources.ApplyResources(this.cmbUnit, "cmbUnit");
            this.cmbUnit.BackgroundImage = null;
            this.cmbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnit.Font = null;
            this.cmbUnit.Items.AddRange(new object[] {
            resources.GetString("cmbUnit.Items"),
            resources.GetString("cmbUnit.Items1"),
            resources.GetString("cmbUnit.Items2")});
            this.cmbUnit.Name = "cmbUnit";
            this.cmbUnit.SelectedIndexChanged += new System.EventHandler(this.cmbUnit_SelectedIndexChanged);
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = null;
            this.btnCancel.AccessibleName = null;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackgroundImage = null;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = null;
            this.btnCancel.Name = "btnCancel";
            // 
            // cmbFields
            // 
            this.cmbFields.AccessibleDescription = null;
            this.cmbFields.AccessibleName = null;
            resources.ApplyResources(this.cmbFields, "cmbFields");
            this.cmbFields.BackgroundImage = null;
            this.cmbFields.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFields.Font = null;
            this.cmbFields.Name = "cmbFields";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // FormRotationType
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.cmbFields);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cmbUnit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.panelAritmetic);
            this.Controls.Add(this.picGeographic);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormRotationType";
            this.Load += new System.EventHandler(this.FromRotationType_Load);
            this.ResumeLayout(false);

		}
		#endregion

        public string FieldName
        {
            get
            {
                if (cmbFields.SelectedItem == null) return "";
                if (((FieldItem)cmbFields.SelectedItem).Field == null) return "";

                return ((FieldItem)cmbFields.SelectedItem).Field.name;
            }
            set
            {
                foreach (FieldItem item in cmbFields.Items)
                {
                    if (item.Field == null && value == "")
                    {
                        cmbFields.SelectedItem = item;
                        break;
                    }
                    else if(item.Field.name==value)
                    {
                        cmbFields.SelectedItem = item;
                        break;
                    }
                }
            }
        }

		public int Mode 
		{
			get { return m_mode; }
			set { m_mode=value; }
		}
		public int Unit
		{
			get { return m_unit; }
			set 
			{
				if(value>2) value=2;
				m_unit=value; 
			}
		}
		private void picGeographic_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			System.Drawing.Graphics gr=e.Graphics;
			System.Drawing.SolidBrush brush=new System.Drawing.SolidBrush(Color.White);
			System.Drawing.Pen pen=new System.Drawing.Pen(Color.Black);
			System.Drawing.Font font=new Font("Arial",7);

			int width=picGeographic.Width,height=picGeographic.Height;
			gr.FillRectangle(brush,0,0,width,height);
			gr.DrawLine(pen,width/2,10,width/2,height-10);
			gr.DrawLine(pen,10,height/2,width-10,height/2);

			brush.Color=Color.Black;
			gr.DrawString(str1,font,brush,width/2+2,7);
			gr.DrawString(str2,font,brush,width-10-gr.MeasureString(str2,font).Width,height/2-10);
			gr.DrawString(str3,font,brush,width/2+2,height-18);
			gr.DrawString(str4,font,brush,10,height/2-10);

			font.Dispose();
			pen.Dispose();
			brush.Dispose();
		}

		private void panelAritmetic_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			System.Drawing.Graphics gr=e.Graphics;
			System.Drawing.SolidBrush brush=new System.Drawing.SolidBrush(Color.White);
			System.Drawing.Pen pen=new System.Drawing.Pen(Color.Black);
			System.Drawing.Font font=new Font("Arial",7);

			int width=panelAritmetic.Width,height=panelAritmetic.Height;
			gr.FillRectangle(brush,0,0,width,height);
			gr.DrawLine(pen,width/2,10,width/2,height-10);
			gr.DrawLine(pen,10,height/2,width-10,height/2);

			brush.Color=Color.Black;
			gr.DrawString(str2,font,brush,width/2+2,7);
			gr.DrawString(str1,font,brush,width-10-gr.MeasureString(str1,font).Width,height/2-10);
			gr.DrawString(str4,font,brush,width/2+2,height-18);
			gr.DrawString(str3,font,brush,10,height/2-10);

			font.Dispose();
			pen.Dispose();
			brush.Dispose();
		}

		private void cmbUnit_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			switch(cmbUnit.SelectedItem.ToString().ToLower()) 
			{
				case "rad":
					str1="0";
					str2="0.5*PI";
					str3="PI";
					str4="1.5*PI";
					break;
				case "deg":
					str1="0";
					str2="90";
					str3="180";
					str4="270";
					break;
				case "gon":
					str1="0";
					str2="100";
					str3="200";
					str4="300";
					break;
			}
			picGeographic.Refresh();
			panelAritmetic.Refresh();
		}

		private void FromRotationType_Load(object sender, System.EventArgs e)
		{
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			Unit=cmbUnit.SelectedIndex;
			if(radioButton1.Checked)
				Mode=0;
			else
				Mode=1;

            if (_symbolRotation != null)
            {
                if (((FieldItem)cmbFields.SelectedItem).Field == null)
                    _symbolRotation.RotationFieldName = "";
                else
                    _symbolRotation.RotationFieldName = ((FieldItem)cmbFields.SelectedItem).Field.name;

                switch(Unit) 
                {
                    case 0:
                        _symbolRotation.RotationUnit = RotationUnit.rad;
                        break;
                    case 1:
                        _symbolRotation.RotationUnit = RotationUnit.deg;
                        break;
                    case 2:
                        _symbolRotation.RotationUnit = RotationUnit.gon;
                        break;
                }
                switch (Mode)
                {
                    case 0:
                        _symbolRotation.RotationType = RotationType.geographic;
                        break;
                    case 1:
                        _symbolRotation.RotationType = RotationType.aritmetic;
                        break;
                }
            }
		}
		
	}

    internal class FieldItem
    {
        IField _field;

        public FieldItem(IField field)
        {
            _field = field;
        }

        public IField Field
        {
            get { return _field; }
        }

        public override string ToString()
        {
            if (_field == null) return "none";
            return _field.aliasname;
        }
    }
}
