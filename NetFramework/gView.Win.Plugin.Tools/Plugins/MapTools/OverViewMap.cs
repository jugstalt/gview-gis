using gView.Framework.Carto;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Plugins.MapTools.Dialogs;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("61301C1E-BC8E-4081-A8BB-65BCC13C89EC")]
    public class OverViewMap : ITool
    {
        private IMapDocument _doc;
        private FormOverviewMap _dlg = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.OverViewMap", "Overview Map Window"); }
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
            if (hook is IMapDocument)
            {
                _doc = hook as IMapDocument;

                if (_doc is IMapDocumentEvents)
                {
                    ((IMapDocumentEvents)_doc).MapScaleChanged += new MapScaleChangedEvent(_doc_MapScaleChanged);
                }
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_doc == null)
            {
                return Task.FromResult(true);
            }

            if (_dlg == null)
            {
                _dlg = new FormOverviewMap(_doc);
                _dlg.FormClosing += new FormClosingEventHandler(dlg_FormClosing);
            }
            _dlg.Show();

            return Task.FromResult(true);
        }

        #endregion

        void dlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dlg = null;
        }

        void _doc_MapScaleChanged(IDisplay sender)
        {
            if (_dlg != null)
            {
                _dlg.DrawMapExtent(sender);
            }
        }
    }
}
