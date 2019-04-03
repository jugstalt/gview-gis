using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.DataSources.Fdb.UI
{
    internal partial class FormSelectFields : Form
    {
        public FormSelectFields(List<IField> fields)
        {
            InitializeComponent();

            if (fields == null) return;

            foreach (IField field in fields)
            {
                if (field == null || 
                    field.type==FieldType.ID ||field.type==FieldType.Shape) continue;
                lstBox.Items.Add(new FieldItem(field), true);
            }
        }

        public FormSelectFields(ITableClass tcFrom, ITableClass tcTo)
        {
            InitializeComponent();

            if (tcFrom == null || tcTo == null || 
                tcFrom.Fields == null || tcTo.Fields == null) return;

            foreach (IField field in tcFrom.Fields.ToEnumerable())
            {
                if (field == null ||
                    field.type == FieldType.ID || field.type == FieldType.Shape ||
                    tcTo.FindField(field.name) != null) continue;
                lstBox.Items.Add(new FieldItem(field), true);
            }
        }

        public List<IField> SelectedFields
        {
            get
            {
                List<IField> selected = new List<IField>();
                foreach (FieldItem item in lstBox.CheckedItems)
                {
                    selected.Add(item.Field);
                }
                return selected;
            }
        }
        private class FieldItem
        {
            private IField _field=null;
            public FieldItem(IField field)
            {
                _field = field;
            }

            public IField Field { get { return _field; } }

            public override string ToString()
            {
                return _field.name + " (" + _field.type.ToString() + ")";
            }
        }
    }
}