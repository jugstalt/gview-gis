using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    internal class HatchStyleTypeEditor : UITypeEditor
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
                Form_UITypeEditor_DashStyle dlg = new Form_UITypeEditor_DashStyle(wfes, (GraphicsEngine.HatchStyle)value);
                dlg.TopLevel = false;
                wfes.DropDownControl(dlg);
                value = dlg.HatchStyle;
            }
            return value;
        }

    }
}
