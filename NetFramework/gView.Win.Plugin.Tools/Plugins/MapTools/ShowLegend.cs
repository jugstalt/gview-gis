using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Plugins.MapTools.Dialogs;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("8874DF04-1B5D-4c22-9913-D1F45B9DC958")]
    public class ShowLegend : ITool
    {
        private IMapDocument _doc = null;
        FormLegend _dlg = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.ShowLegend", "Legend Window..."); }
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
                    ((IMapDocumentEvents)_doc).AfterSetFocusMap += new AfterSetFocusMapEvent(_doc_AfterSetFocusMap);
                    ((IMapDocumentEvents)_doc).LayerAdded += new LayerAddedEvent(_doc_LayerAdded);
                    ((IMapDocumentEvents)_doc).LayerRemoved += new LayerRemovedEvent(_doc_LayerRemoved);
                    ((IMapDocumentEvents)_doc).MapAdded += new MapAddedEvent(_doc_MapAdded);
                    ((IMapDocumentEvents)_doc).MapDeleted += new MapDeletedEvent(_doc_MapDeleted);
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
                _dlg = new FormLegend(_doc);
                _dlg.FormClosing += new FormClosingEventHandler(dlg_FormClosing);
            }
            _dlg.Show();

            return Task.FromResult(true);
        }

        #endregion

        async private Task RefreshLegend()
        {
            if (_dlg == null)
            {
                return;
            }

            await _dlg.RefreshLegend();
        }
        void dlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dlg = null;
        }

        async void _doc_MapScaleChanged(IDisplay sender)
        {
            await RefreshLegend();
        }

        async void _doc_MapDeleted(IMap map)
        {
            await RefreshLegend();
        }

        async void _doc_MapAdded(IMap map)
        {
            await RefreshLegend();
        }

        async void _doc_LayerRemoved(IMap sender, ILayer layer)
        {
            await RefreshLegend();
        }

        async void _doc_LayerAdded(IMap sender, ILayer layer)
        {
            await RefreshLegend();
        }

        async void _doc_AfterSetFocusMap(IMap map)
        {
            await RefreshLegend();
        }
    }
}
