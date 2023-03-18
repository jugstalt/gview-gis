using gView.Framework.Symbology.UI.Controls;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace gView.Framework.Symbology.UI
{

    internal class ColorGradientEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            FormColorGradient dlg = new FormColorGradient();
            ColorGradient gradient = new ColorGradient(GraphicsEngine.ArgbColor.Red, GraphicsEngine.ArgbColor.Blue);

            if (context.Instance is GradientFillSymbol)
            {
                gradient =
                dlg.ColorGradient = ((GradientFillSymbol)context.Instance).ColorGradient;
            }
            if (context.Instance is CustomClass &&
                ((CustomClass)context.Instance).ObjectInstance is GradientFillSymbol)
            {
                gradient =
                dlg.ColorGradient = ((GradientFillSymbol)((CustomClass)context.Instance).ObjectInstance).ColorGradient;
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.ColorGradient;
            }
            return gradient;
        }
    }
}
