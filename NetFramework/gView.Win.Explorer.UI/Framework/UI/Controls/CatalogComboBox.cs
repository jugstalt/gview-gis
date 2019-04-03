using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Management;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Explorer.UI;
using gView.Framework.UI.Dialogs;

namespace gView.Framework.UI.Controls
{
    internal partial class CatalogComboBox : UserControl
    {
        public delegate void SelectedItemChangedEvent(CatalogComboItem item);
        public event SelectedItemChangedEvent SelectedItemChanged = null;

        public CatalogComboBox()
        {
            InitializeComponent();

            //if (ExplorerImageList.List.CountImages == 0)
            //{
            //    ExplorerImageList.List.AddImages(imageList1);
            //}
            //if (FormExplorer.globalImageList == null) FormExplorer.globalImageList = imageList1;

            using (Graphics graphics = this.CreateGraphics())
            {
                this.FontScaleFactor = graphics.DpiX / 96f;
            }

            cmbCatalog.ItemHeight = (int)(cmbCatalog.ItemHeight * this.FontScaleFactor);
        }

        private float FontScaleFactor { get; set; }

        private void CatalogComboBox_Load(object sender, EventArgs e)
        {

        }

        public void InitComboBox()
        {
            cmbCatalog.Items.Clear();

            ComputerObject computer = new ComputerObject(null);
            cmbCatalog.Items.Add(new ExplorerObjectComboItem(computer.Name, 0, 0, computer));

            foreach (IExplorerObject exObject in computer.ChildObjects)
            {
                cmbCatalog.Items.Add(
                    new DriveComboItem(
                        exObject.Name,
                        ((exObject is DriveObject) ? 1 : 0),
                        ((exObject is DriveObject) ? ((DriveObject)exObject).ImageIndex : gView.Explorer.UI.Framework.UI.ExplorerIcons.ImageIndex(exObject)),
                        exObject
                        ));
            }

            //cmbCatalog.Items.Add(new CatalogComboItem("Computer", 0, 0, null));

            //ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT Name,DriveType From Win32_LogicalDisk ");
            //ManagementObjectCollection collection = query.Get();

            //foreach (ManagementObject manObject in collection)
            //{
            //    DriveObject exObject = new DriveObject(manObject["Name"].ToString(), (uint)manObject["DriveType"]);
            //    cmbCatalog.Items.Add(new DriveComboItem(exObject.Name, 1, exObject.ImageIndex, exObject));
            //}

            //ComponentManager compMan = new ComponentManager();
            //foreach (XmlNode exObjectNode in compMan.ExplorerObjects)
            //{
            //    IExplorerObject exObject = (IExplorerObject)compMan.getComponent(exObjectNode);
            //    if (!(exObject is IExplorerGroupObject)) continue;

            //    cmbCatalog.Items.Add(new ExplorerObjectComboItem(exObject.Name, 0, FormExplorer.ImageIndex(exObject), exObject));
            //}

            cmbCatalog.SelectedIndex = 0;
        }

        public void AddChildNode(IExplorerObject exObject)
        {
            int index = cmbCatalog.SelectedIndex;
            if (index == -1) return;

            int level = ((CatalogComboItem)cmbCatalog.SelectedItem).Level;
            if (exObject is DirectoryObject)
            {
                cmbCatalog.Items.Insert(index + 1, new DirectoryComboItem(exObject.Name, level + 1, gView.Explorer.UI.Framework.UI.ExplorerIcons.ImageIndex(exObject), exObject));
            }
            else
            {
                cmbCatalog.Items.Insert(index + 1, new ExplorerObjectComboItem(exObject.Name, level + 1, gView.Explorer.UI.Framework.UI.ExplorerIcons.ImageIndex(exObject), exObject));
            }
            cmbCatalog.SelectedIndex = index + 1;
        }

        public void MoveUp()
        {
            if (cmbCatalog.SelectedIndex < 1) return;
            if (cmbCatalog.SelectedItem is DriveComboItem) return;

            if (cmbCatalog.SelectedItem is CatalogComboItem)
            {
                CatalogComboItem item = (CatalogComboItem)cmbCatalog.SelectedItem;
                if (item.Level > 0)
                    cmbCatalog.SelectedIndex--;
            }
        }

        public IExplorerObject SelectedExplorerObject
        {
            get
            {
                if (cmbCatalog.SelectedItem is CatalogComboItem)
                {
                    return ((CatalogComboItem)cmbCatalog.SelectedItem).ExplorerObject;
                }
                return null;
            }
        }
        private void cmbCatalog_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= cmbCatalog.Items.Count) return;
            if (!(cmbCatalog.Items[e.Index] is CatalogComboItem)) return;

            //DrawItemState.
            CatalogComboItem item = (CatalogComboItem)cmbCatalog.Items[e.Index];
            int level = item.Level;
            if ((e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit)
            {
                level = 0;
            }
            
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                Rectangle rect = new Rectangle(e.Bounds.X + level * 11 + 18, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected &&
                    (e.State & DrawItemState.ComboBoxEdit) != DrawItemState.ComboBoxEdit)
                {
                    brush.Color = Color.DarkBlue;
                    e.Graphics.FillRectangle(brush, rect);
                    brush.Color = Color.White;
                }
                else
                {
                    brush.Color = Color.White;
                    e.Graphics.FillRectangle(brush, rect);
                    brush.Color = Color.Black;
                }
                try
                {
                    Image image = ExplorerImageList.List[item.ImageIndex];
                    e.Graphics.DrawImage(image, e.Bounds.X + level * 11 + 3, e.Bounds.Y);
                }
                catch { }
                e.Graphics.DrawString(item.ToString(), cmbCatalog.Font, brush, e.Bounds.X + level * 11 + 20, e.Bounds.Y+2);
            }
        }

        private void cmbCatalog_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCatalog.SelectedItem is CatalogComboItem)
            {
                CatalogComboItem item = (CatalogComboItem)cmbCatalog.SelectedItem;
                List<CatalogComboItem> delete = new List<CatalogComboItem>();
                foreach (CatalogComboItem i in cmbCatalog.Items)
                {
                    if (i is DriveComboItem) continue;
                    if (i.Level > item.Level)
                        delete.Add(i);
                }
                foreach (CatalogComboItem i in delete)
                    cmbCatalog.Items.Remove(i);

                if (SelectedItemChanged != null) SelectedItemChanged((CatalogComboItem)cmbCatalog.SelectedItem);
            }
            else
            {
                if (SelectedItemChanged != null) SelectedItemChanged(null);
            }
        }
    }

    internal class CatalogComboItem
    {
        private string _text;
        private int _level, _imageIndex;
        IExplorerObject _exObject;

        public CatalogComboItem(string text, int level, int imageIndex, IExplorerObject exObject)
        {
            _text = text;
            _level = level;
            _imageIndex = imageIndex;
            _exObject = exObject;
        }

        public int Level
        {
            get { return _level; }
        }

        public int ImageIndex
        {
            get { return _imageIndex; }
        }

        public IExplorerObject ExplorerObject
        {
            get { return _exObject; }
        }

        public override string ToString()
        {
            return _text;
        }
    }

    internal class DriveComboItem : CatalogComboItem
    {
        public DriveComboItem(string text, int level, int imageIndex, IExplorerObject exObject)
            : base(text, level, imageIndex,exObject)
        {
        }
    }

    internal class DirectoryComboItem : CatalogComboItem
    {
        public DirectoryComboItem(string text, int level, int imageIndex, IExplorerObject exObject)
            : base(text, level, imageIndex, exObject)
        {
        }
    }

    internal class ExplorerObjectComboItem : CatalogComboItem
    {
        public ExplorerObjectComboItem(string text, int level, int imageIndex, IExplorerObject exObject)
            : base(text, level, imageIndex, exObject)
        {
        }
    }
}
