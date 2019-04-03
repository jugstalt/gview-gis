using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Plugins.MapTools.Dialogs;
using gView.system.UI;

namespace gView.Plugins.MapTools.Controls
{
    internal partial class IdentifyResultControl : UserControl, IDockableToolWindow
    {
        public enum DisplayMode { Features, Text, HTML, Xml }
        internal IMapDocument _doc;

        public IdentifyResultControl()
        {
            InitializeComponent();
        }

        #region IDockableWindow Members

        private DockWindowState _dockState = DockWindowState.none;
        public DockWindowState DockableWindowState
        {
            get
            {
                return _dockState;
            }
            set
            {
                _dockState = value;
            }
        }

        public Image Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.info; }
        }
        #endregion

        #region Properties

        public IMapDocument MapDocument
        {
            set
            {
                _doc = value;
            }
            get
            {
                return _doc;
            }
        }

        public DisplayMode ShowMode
        {
            set
            {
                /*
                tabControl1.TabPages.Remove(tabPageFeatures);
                tabControl1.TabPages.Remove(tabPageHTML);
                tabControl1.TabPages.Remove(tabPageText);

                switch (value)
                {
                    case ShowMode.Features:
                        tabControl1.TabPages.Add(tabPageFeatures);
                        break;
                    case ShowMode.HTML:
                        tabControl1.TabPages.Add(tabPageHTML);
                        break;
                    case ShowMode.Text:
                        tabControl1.TabPages.Add(tabPageText);
                        break;
                }
                 * */
            }
        }
        public string IdentifyText
        {
            get { /*return txtIdentify.Text;*/ return String.Empty; }
            set
            {
                /*
                txtIdentify.Text = value.Replace("\r\n", "\n").Replace("\n", "\r\n");

                if (!String.IsNullOrEmpty(txtIdentify.Text) &&
                    !tabControl1.TabPages.Contains(tabPageText))
                {
                    tabControl1.TabPages.Add(tabPageText);
                }
                 * */
            }
        }
        public string IdentifyUrl
        {
            get
            {
                /*
                if (webBrowserControl.Url == null)
                    return String.Empty;

                return webBrowserControl.Url.AbsoluteUri;
                 * */
                return String.Empty;
            }
            set
            {
                /*
                try
                {
                    if (tabControl1.TabPages.Contains(tabPageHTML)) return;

                    if (String.IsNullOrEmpty(value))
                    {
                        webBrowserControl.Url = null;
                        tabControl1.TabPages.Remove(tabPageHTML);
                    }
                    else
                    {
                        webBrowserControl.Url = new Uri(value);
                        if (!tabControl1.TabPages.Contains(tabPageHTML))
                        {
                            tabControl1.TabPages.Add(tabPageHTML);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR: " + ex.Message + "\n" + value);
                }
                 * */
            }
        }
        public List<IDatasetElement> AllQueryableLayers
        {
            get
            {
                if (_doc == null && !(_doc.Application is IGUIApplication)) return new List<IDatasetElement>();
                QueryThemeCombo queryCombo = ((IGUIApplication)_doc.Application).Tool(new Guid("51A2CF81-E343-4c58-9A42-9207C8DFBC01")) as QueryThemeCombo;
                if (queryCombo == null) return new List<IDatasetElement>();

                return queryCombo.QueryableDatasetElements;
            }
        }

        public IdentifyMode Mode
        {
            get
            {
                if (_doc == null && !(_doc.Application is IGUIApplication)) return IdentifyMode.visible;
                QueryThemeCombo queryCombo = ((IGUIApplication)_doc.Application).Tool(new Guid("51A2CF81-E343-4c58-9A42-9207C8DFBC01")) as QueryThemeCombo;
                if (queryCombo == null) return IdentifyMode.visible;

                return queryCombo.Mode;
            }
        }

        #endregion

        #region Memebers

        public void Clear()
        {
            gridFeatureProperties.SelectedObject = null;
            cmbFeatures.Items.Clear();
            this.ShowMode = DisplayMode.Features;
        }

        public void SetLocation(double x, double y)
        {
            txtLocation.Text = "Location: (" + x.ToString() + "  " + y.ToString() + ")";
        }

        public void AddFeature(IFeature feature, ISpatialReference sRef, IFeatureLayer layer)
        {
            AddFeature(feature, sRef, layer, "???");
        }
        public void AddFeature(IFeature feature, ISpatialReference sRef, IFeatureLayer layer, string Category)
        {
            AddFeature(feature, sRef, layer, Category, null, null);
        }
        public void AddFeature(IFeature feature, ISpatialReference sRef, IFeatureLayer layer, string Category, IFields fields, IField primaryDisplayField)
        {
            FeatureItem fItem = new FeatureItem()
            {
                Feature = feature,
                FeatureLayer = layer,
                SpatialReferenece = sRef,
                Category = Category,
                Fields = fields,
                PrimaryDisplayField = primaryDisplayField
            };

            cmbFeatures.Items.Add(fItem);
        }

        public void ShowResult()
        {
            if (cmbFeatures.Items.Count > 0)
                cmbFeatures.SelectedIndex = 0;

            //if (treeObjects.Nodes.Count == 0)
            //{
            //    if (tabControl1.TabPages.Contains(tabPageText))
            //        tabControl1.SelectedIndex = tabControl1.TabPages.IndexOf(tabPageText);
            //    else if (tabControl1.TabPages.Contains(tabPageHTML))
            //        tabControl1.SelectedIndex = tabControl1.TabPages.IndexOf(tabPageHTML);
            //}
            //else
            //{
            //    if (tabControl1.TabPages.Contains(tabPageFeatures))
            //        tabControl1.SelectedIndex = tabControl1.TabPages.IndexOf(tabPageFeatures);
            //}
        }

        public void WriteFeatureCount()
        {
            //foreach (TreeNode node in treeObjects.Nodes)
            //{
            //    if (node is CategoryTreeNode)
            //    {
            //        int count = node.Nodes.Count;
            //        node.Text = ((CategoryTreeNode)node).Category + " (" + count.ToString() + ((count == 1) ? " Feature)" : " Features)");
            //    }
            //}
        }

        #endregion

        #region Item Classes

        private class FeatureItem
        {
            public IFeature Feature;
            public IFeatureLayer FeatureLayer;
            public ISpatialReference SpatialReferenece;
            public string Category;
            public IFields Fields;
            public IField PrimaryDisplayField;

            public override string ToString()
            {
                return Category;
            }
        }

        #endregion

        #region UI Events

        private void cmbFeatures_SelectedIndexChanged(object sender, EventArgs e)
        {
            FeatureItem fItem = cmbFeatures.SelectedItem as FeatureItem;
            if (fItem == null)
            {
                gridFeatureProperties.SelectedObject = null;
                return;
            }
            else
            {
                DictionaryPropertyGridAdapter dictAdapter = new DictionaryPropertyGridAdapter(fItem.Feature, fItem.Fields);
                gridFeatureProperties.SelectedObject = dictAdapter;
            }
        }

        #endregion
    }
}
