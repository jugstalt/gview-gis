using gView.Framework.system;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("7DC2ECC7-DC8C-49B4-9AA6-DBEEB0026E0A")]
    public class SaveDocumentBackstage : SaveDocument
    {
        public override object Image
        {
            get
            {
                return gView.Win.Plugin.Tools.Properties.Resources.save_16_w;
            }
        }
    }
}
