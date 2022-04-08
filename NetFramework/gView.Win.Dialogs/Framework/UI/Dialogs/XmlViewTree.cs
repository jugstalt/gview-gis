using System.Windows.Forms;
using System.Xml;

namespace gView.Framework.UI.Dialogs
{
    public class XmlViewTree : TreeView
    {
        private XmlDocument _doc = null;
        private System.ComponentModel.IContainer components;

        public XmlViewTree()
            : base()
        {
        }
        public XmlDocument XmlDocument
        {
            get
            {
                return _doc;
            }
            set
            {
                _doc = value;
                BuildTree();
            }
        }

        private bool _showAttributes = true;
        public bool ShowAttributes
        {
            get { return _showAttributes; }
            set { _showAttributes = value; }
        }

        public void BuildTree()
        {
            if (this.ImageList == null)
            {
                this.ImageList = new ImageList();
                this.ImageList.Images.Add(global::gView.Win.Dialogs.Properties.Resources.img_18);
                this.ImageList.Images.Add(global::gView.Win.Dialogs.Properties.Resources.img_9);
            }
            this.Nodes.Clear();

            if (_doc == null)
            {
                return;
            }

            foreach (XmlNode child in _doc.ChildNodes)
            {
                AppendNode(null, child);
            }
        }

        private void AppendNode(TreeNode parent, XmlNode xmlNode)
        {
            if (xmlNode == null)
            {
                return;
            }

            TreeNode node = new TreeNode(xmlNode.Name, 0, 0);
            if (parent == null)
            {
                this.Nodes.Add(node);
            }
            else
            {
                parent.Nodes.Add(node);
            }

            if (xmlNode.Attributes != null && _showAttributes)
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    TreeNode attr = new TreeNode(
                        attribute.Name + "=" + attribute.Value, 1, 1);
                    node.Nodes.Add(attr);
                }
            }
            if (xmlNode.ChildNodes != null)
            {
                foreach (XmlNode child in xmlNode.ChildNodes)
                {
                    if (child.Name == "#text")
                    {
                        node.Text += ": " + child.InnerText;
                    }
                    else
                    {
                        AppendNode(node, child);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // XmlViewTree
            // 
            this.LineColor = System.Drawing.Color.Black;
            this.ResumeLayout(false);

        }
    }
}
