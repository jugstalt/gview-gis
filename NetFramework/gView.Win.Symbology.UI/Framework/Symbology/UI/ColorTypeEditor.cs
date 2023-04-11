using gView.Framework.Symbology.UI.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    internal class ColorTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService wfes = provider.GetService(
                typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (wfes != null)
            {
                OfficeColorPicker picker = new OfficeColorPicker(wfes, (Color)value);
                picker.AllowNoColor = true;
                picker.Height = picker.PreferredHeight;
                wfes.DropDownControl(picker);
                return picker.Color;
            }

            return value;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush((Color)e.Value))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

    }
}
