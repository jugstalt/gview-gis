using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    internal class DashStyleTypeEditor : UITypeEditor
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
                Form_UITypeEditor_DashStyle dlg = new Form_UITypeEditor_DashStyle(wfes, (GraphicsEngine.LineDashStyle)value);
                dlg.TopLevel = false;
                wfes.DropDownControl(dlg);
                value = dlg.DashStyle;
            }
            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            using (Pen pen = new Pen(Color.Black, 1))
            {
                pen.DashStyle = (DashStyle)e.Value;
                e.Graphics.DrawLine(pen, e.Bounds.Left + 2, e.Bounds.Height / 2, e.Bounds.Right - 4, e.Bounds.Height / 2);
            }
        }
    }
}
