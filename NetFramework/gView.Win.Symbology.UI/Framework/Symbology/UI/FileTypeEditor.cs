using gView.Framework.Carto.UI;
using gView.Framework.UI.Symbology.Dialogs;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace gView.Framework.Symbology.UI
{
    internal class FileTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {

            string filename = String.Empty;
            if (context.Instance is RasterMarkerSymbol)
            {
                filename = ((RasterMarkerSymbol)context.Instance).Filename;
            }
            if (context.Instance is CustomClass &&
                ((CustomClass)context.Instance).ObjectInstance is RasterMarkerSymbol)
            {
                filename = ((RasterMarkerSymbol)((CustomClass)context.Instance).ObjectInstance).Filename;
            }

            #region Check Resources First

            var currentMap = SymbolPreview.CurrentMap;
            if (currentMap?.ResourceContainer != null && currentMap.ResourceContainer.HasResources)
            {
                var resourcePicker = new FormMapResourcePicker(currentMap);
                resourcePicker.SetAbortButtonText("Or Select File...");

                switch (resourcePicker.ShowDialog())
                {
                    case DialogResult.OK:
                        return $"resource:{resourcePicker.SelectedResourceName}";
                    case DialogResult.Abort:
                        // Open Filedialog
                        break;
                    default:
                        return filename;
                }
            }

            #endregion

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = filename;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
            }
            return filename;
        }
    }
}
