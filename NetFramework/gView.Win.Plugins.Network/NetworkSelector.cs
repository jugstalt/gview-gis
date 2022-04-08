using gView.Framework.Data;
using gView.Framework.Globalisation;
using gView.Framework.Network;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.Network
{
    [RegisterPlugInAttribute("44762AEE-4F9C-4039-9577-372DC106B1C8")]
    public class NetworkSelector : ITool, IToolItem
    {
        private IMapDocument _doc = null;
        private ToolStripComboBox _combo;
        private Module _module = null;

        public NetworkSelector()
        {
            _combo = new ToolStripComboBox();
            _combo.DropDownStyle = ComboBoxStyle.DropDownList;
            _combo.Width = 300;
            _combo.SelectedIndexChanged += new EventHandler(_combo_SelectedIndexChanged);
        }

        void _combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_module != null)
            {
                if (_combo.SelectedItem is NetworkClassItem)
                {
                    _module.SelectedNetworkFeatureClass = ((NetworkClassItem)_combo.SelectedItem).NetworkFeatureClass;
                }
                else
                {
                    _module.SelectedNetworkFeatureClass = null;
                }
            }
        }

        public void FillCombo()
        {
            _combo.Items.Clear();

            if (_doc == null || _doc.FocusMap == null)
            {
                return;
            }

            foreach (IDatasetElement layer in _doc.FocusMap.MapElements)
            {
                if (!(layer is IFeatureLayer))
                {
                    continue;
                }

                if (layer.Class is INetworkFeatureClass)
                {
                    _combo.Items.Add(new NetworkClassItem(layer.Title, (INetworkFeatureClass)layer.Class));
                }
            }
            if (_combo.Items.Count > 0)
            {
                _combo.SelectedIndex = 0;
            }
        }

        #region ITool Member

        public string Name
        {
            get { return "Network Selector"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
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
                _doc = (IMapDocument)hook;
                _doc.AfterSetFocusMap += new AfterSetFocusMapEvent(_doc_AfterSetFocusMap);
                _doc.LayerAdded += new gView.Framework.Carto.LayerAddedEvent(_doc_LayerAdded);
                _doc.LayerRemoved += new gView.Framework.Carto.LayerRemovedEvent(_doc_LayerRemoved);

                _module = Module.GetModule(_doc);
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Member

        public System.Windows.Forms.ToolStripItem ToolItem
        {
            get { return _combo; }
        }

        #endregion

        #region MapDocumentEvents
        void _doc_LayerRemoved(gView.Framework.Carto.IMap sender, gView.Framework.Data.ILayer layer)
        {
            FillCombo();
        }

        void _doc_LayerAdded(gView.Framework.Carto.IMap sender, gView.Framework.Data.ILayer layer)
        {
            FillCombo();
        }

        void _doc_AfterSetFocusMap(gView.Framework.Carto.IMap map)
        {
            FillCombo();
        }
        #endregion

        #region ItemClasses
        private class NetworkClassItem
        {
            private string _name;
            private INetworkFeatureClass _nfc;

            public NetworkClassItem(string name, INetworkFeatureClass nfc)
            {
                _name = name;
                _nfc = nfc;
            }

            public INetworkFeatureClass NetworkFeatureClass
            {
                get { return _nfc; }
            }
            public override string ToString()
            {
                return _name;
            }
        }
        #endregion

        public INetworkFeatureClass SelectedNetworkFeatureClass
        {
            get
            {
                if (_combo.SelectedItem is NetworkClassItem)
                {
                    return ((NetworkClassItem)_combo.SelectedItem).NetworkFeatureClass;
                }

                return null;
            }
        }
    }

    [RegisterPlugInAttribute("002124CB-804E-449e-BA7B-A3F3CBBBD154")]
    public class NetworkSelectorLabel : ITool, IToolItem
    {
        #region ITool Member

        public string Name
        {
            get { return "Network Selector Label"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
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
                return new ToolStripLabel(LocalizedResources.GetResString("String.Network", "Network"));
            }
        }

        #endregion
    }
}
