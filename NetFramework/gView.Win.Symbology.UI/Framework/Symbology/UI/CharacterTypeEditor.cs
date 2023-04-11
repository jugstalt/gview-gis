using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    internal class CharacterTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            GraphicsEngine.Abstraction.IFont iFont = null;
            if (context.Instance is TrueTypeMarkerSymbol)
            {
                iFont = ((TrueTypeMarkerSymbol)context.Instance).Font;
            }
            if (context.Instance is CustomClass &&
                ((CustomClass)context.Instance).ObjectInstance is TrueTypeMarkerSymbol)
            {
                iFont = ((TrueTypeMarkerSymbol)((CustomClass)context.Instance).ObjectInstance).Font;
            }

            if (iFont == null)
            {
                return value;
            }

            IWindowsFormsEditorService wfes = provider.GetService(
                typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (wfes != null)
            {
                Form_UITypeEditor_Character dlg = new Form_UITypeEditor_Character(wfes, iFont, (byte)value);
                wfes.DropDownControl(dlg.panelChars);
                return dlg.Charakter;
            }
            return value;
        }
    }
}
