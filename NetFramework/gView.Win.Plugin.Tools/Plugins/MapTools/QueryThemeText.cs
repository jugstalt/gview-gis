using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("9AADD17B-CDD0-4111-BBC5-E31E060CE210")]
    public class QueryThemeText : gView.Framework.UI.ITool, gView.Framework.UI.IToolItem
    {
        #region ITool Member

        public string Name
        {
            get { return "ScaleText"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Member

        public ToolStripItem ToolItem
        {
            get
            {
                return new ToolStripLabel(
                    LocalizedResources.GetResString("Text.Scale", "Scale:")
                    );
            }
        }

        #endregion
    }
}
