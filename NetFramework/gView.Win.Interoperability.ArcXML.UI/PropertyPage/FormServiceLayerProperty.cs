using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Interoperability.ArcXML.Dataset;
using System.Xml;

namespace gView.Interoperability.ArcXML.UI.PropertyPage
{
    [gView.Framework.system.RegisterPlugIn("ED40997C-94EE-499f-8092-72667C1DB4AB")]
    public partial class FormServiceLayerProperty : Form,ILayerPropertyPage
    {
        private XmlDocument _properties = null;
        
        public FormServiceLayerProperty()
        {
            InitializeComponent();
        }

        #region Tree
        private void BuildTree()
        {
            tvProperties.Nodes.Clear();

            if (_properties == null) return;

            foreach (XmlNode childNode in _properties.ChildNodes)
            {
                AppendNode(null, childNode);
            }
            
        }
        private void AppendNode(TreeNode parent, XmlNode xmlNode)
        {
            if (xmlNode == null) return;

            TreeNode node = new TreeNode(xmlNode.Name, 0, 0);
            if (parent == null)
                tvProperties.Nodes.Add(node);
            else
                parent.Nodes.Add(node);

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                AppendNode(node, childNode);
            }
            foreach (XmlAttribute attribute in xmlNode.Attributes)
            {
                node.Nodes.Add(new TreeNode(attribute.Name, 1, 1));
            }
        }

        private string XPath(TreeNode node)
        {
            if (node == null) return "";
            return XPath(node.Parent) + ((node.Parent!=null) ? "/" : "") + node.Text;
        }

        private XmlAttribute _selectedAttribute = null;
        private void tvProperties_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _selectedAttribute=null;
            if (e.Node.ImageIndex == 1)
            {
                string path = XPath(e.Node.Parent);
                XmlNode xmlNode = _properties.SelectSingleNode(path);
                if (xmlNode != null)
                    _selectedAttribute = xmlNode.Attributes[e.Node.Text];
            }

            if (_selectedAttribute != null)
            {
                txtAttributeText.Text = _selectedAttribute.Value;
                txtAttributeText.Enabled = true;
            }
            else
            {
                txtAttributeText.Text = "";
                txtAttributeText.Enabled = false;
            }
        }

        #endregion

        #region ILayerPropertyPage Member

        public Panel PropertyPage(IDataset dataset, gView.Framework.Data.ILayer layer)
        {
            if (ShowWith(dataset, layer))
            {
                _properties = ((ArcIMSDataset)layer.Class.Dataset).Properties;
                BuildTree();
                return panel1;
            }
            else
            {
                return null;
            }
        }

        public bool ShowWith(IDataset dataset, gView.Framework.Data.ILayer layer)
        {
            if (layer is IWebServiceLayer &&
                ((IWebServiceLayer)layer).Class is ArcIMSClass &&
                ((IWebServiceLayer)layer).Class.Dataset is ArcIMSDataset)
            {
                return true;
            }
            return false;
        }

        public string Title
        {
            get { return "ArcXML Properties"; }
        }

        public void Commit()
        {
            
        }

        #endregion

        private void txtAttributeText_TextChanged(object sender, EventArgs e)
        {
            if (_selectedAttribute != null)
                _selectedAttribute.Value = txtAttributeText.Text;
        }
    }
}