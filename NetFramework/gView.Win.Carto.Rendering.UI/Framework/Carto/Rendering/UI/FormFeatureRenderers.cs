using gView.Framework.Data;
using gView.Framework.system;
using System.Windows.Forms;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class FormFeatureRenderers : Form
    {
        public enum RendererType { featureRenderer, labelRenderer }
        private RendererType _type = RendererType.featureRenderer;
        private IFeatureRenderer _selectedRenderer = null;
        private ILabelRenderer _selectedLabelRenderer = null;
        private IFeatureLayer _layer;

        public FormFeatureRenderers(IFeatureLayer layer)
        {
            InitializeComponent();

            btnOK.Enabled = false;
            _layer = layer;
            MakeTree();
        }

        public FormFeatureRenderers(IFeatureLayer layer, RendererType type)
        {
            InitializeComponent();

            btnOK.Enabled = false;
            _type = type;
            _layer = layer;
            MakeTree();
        }

        private void MakeTree()
        {
            PlugInManager compManager = new PlugInManager();
            if (_type == RendererType.featureRenderer)
            {
                foreach (var pluginType in compManager.GetPlugins(Plugins.Type.IFeatureRenderer))
                {
                    IFeatureRenderer renderer = compManager.CreateInstance<IFeatureRenderer>(pluginType);
                    if (renderer == null || !renderer.CanRender(_layer, null))
                    {
                        continue;
                    }

                    TreeNode parent = null;
                    foreach (TreeNode cat in tvRenderer.Nodes)
                    {
                        if (cat.Text == renderer.Category)
                        {
                            parent = cat;
                            break;
                        }
                    }
                    if (parent == null)
                    {
                        parent = new TreeNode(renderer.Category);
                        tvRenderer.Nodes.Add(parent);
                    }

                    TreeNode rNode = new RendererNode(renderer);
                    parent.Nodes.Add(rNode);
                }
            }
            else if (_type == RendererType.labelRenderer)
            {
                TreeNode parent = new TreeNode("Label Renderers");
                foreach (var pluginType in compManager.GetPlugins(Plugins.Type.ILabelRenderer))
                {
                    ILabelRenderer renderer = compManager.CreateInstance<ILabelRenderer>(pluginType);
                    if (renderer == null || !renderer.CanRender(_layer, null))
                    {
                        continue;
                    }

                    parent.Nodes.Add(new LabelRendererNode(renderer));
                }
                tvRenderer.Nodes.Add(parent);
            }

            foreach (TreeNode parent in tvRenderer.Nodes)
            {
                parent.Expand();
            }
        }

        private class RendererNode : TreeNode
        {
            IFeatureRenderer _renderer;

            public RendererNode(IFeatureRenderer renderer)
            {
                _renderer = renderer;

                if (_renderer != null)
                {
                    base.Text = renderer.Name;
                }
                else
                {
                    base.Text = "Default";
                }
            }
            public IFeatureRenderer FeatureRenderer
            {
                get { return _renderer; }
            }
        }

        private class LabelRendererNode : TreeNode
        {
            ILabelRenderer _renderer;

            public LabelRendererNode(ILabelRenderer renderer)
            {
                _renderer = renderer;

                if (_renderer != null)
                {
                    base.Text = renderer.Name;
                }
                else
                {
                    base.Text = "Default";
                }
            }
            public ILabelRenderer LabelRenderer
            {
                get { return _renderer; }
            }
        }

        private void tvRenderer_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node is RendererNode)
            {
                btnOK.Enabled = true;
                if (_selectedRenderer != ((RendererNode)e.Node).FeatureRenderer)
                {
                    //if (_selectedRenderer != null) _selectedRenderer.Release();
                    _selectedRenderer = ((RendererNode)e.Node).FeatureRenderer;
                }
            }
            else if (e.Node is LabelRendererNode)
            {
                btnOK.Enabled = true;
                if (_selectedLabelRenderer != ((LabelRendererNode)e.Node).LabelRenderer)
                {
                    //if (_selectedLabelRenderer != null) _selectedRenderer.Release();
                    _selectedLabelRenderer = ((LabelRendererNode)e.Node).LabelRenderer;
                }
            }
            else
            {
                btnOK.Enabled = false;
                _selectedRenderer = null;
            }
        }

        public IFeatureRenderer SelectedFeatureRenderer
        {
            get { return _selectedRenderer; }
        }

        public ILabelRenderer SelectedLabelRenderer
        {
            get { return _selectedLabelRenderer; }
        }
    }
}