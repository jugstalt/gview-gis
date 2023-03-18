using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    internal class PenWidthTypeEditor : UITypeEditor
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
                gView.Framework.Symbology.UI.Controls.OfficeLineWidthPicker picker = new gView.Framework.Symbology.UI.Controls.OfficeLineWidthPicker(wfes);
                picker.Height = picker.PreferredHeight;
                wfes.DropDownControl(picker);
                return picker.PenWidth != -1 ? picker.PenWidth : value;
            }
            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {

            using (Pen pen = new Pen(Color.Black, Math.Min(e.Bounds.Height, (float)e.Value)))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left + 1, e.Bounds.Height / 2, e.Bounds.Right - 1, e.Bounds.Height / 2);
            }
        }
    }
}
