using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using gView.Framework.Symbology;
using gView.Framework.IO;
using gView.Framework.Carto;
using gView.Framework.Carto.UI;
using gView.Framework.Carto.Rendering;
using gView.Framework.Symbology.UI;

namespace gView.Framework.UI.Controls
{
    /// <summary>
    /// Zusammenfassung für SymbolsListView.
    /// </summary>
    public class SymbolsListView : System.Windows.Forms.UserControl
    {
        public delegate void SymbolChanged(string key, ISymbol symbol);
        public event SymbolChanged OnSymbolChanged;
        public delegate void LabelChanged(ISymbol symbol, int nr, string label);
        public event LabelChanged OnLabelChanged;
        public delegate void KeyChanged(string oldKey, string newKey);
        public event KeyChanged OnKeyChanged;
        public delegate void DeleteItem(string key);
        public event DeleteItem OnDeleteItem;
        public delegate void SymbolClicked(ISymbol symbol);
        public event SymbolClicked OnSymbolClicked;
        public event EventHandler AfterLegendOrdering = null;
        public event EventHandler SelectedIndexChanged = null;

        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ListView list;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList imageList1;
        private object lockThis = new object();
        private TextBox _box;

        //private TextBox _box = new TextBox();

        public SymbolsListView()
        {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            /*
            list.Controls.Add(_box);
            _box.BorderStyle = BorderStyle.FixedSingle;
            _box.Show();
            _box.Visible = false;
            */
        }

        public void addSymbol(ISymbol symbol, string[] labels)
        {
            Bitmap bm = new Bitmap(imageList1.ImageSize.Width, imageList1.ImageSize.Height);
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm);
            SymbolPreview.Draw(gr, new Rectangle(0, 0, bm.Width, bm.Height), symbol);
            gr.Dispose();
            gr = null;
            imageList1.Images.Add(bm);

            list.Items.Add(new SymbolListViewItem(symbol, labels, imageList1.Images.Count - 1));
        }

        public void addSymbol(ISymbol symbol, string[] labels, object userObject)
        {
            Bitmap bm = new Bitmap(imageList1.ImageSize.Width, imageList1.ImageSize.Height);
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm);
            SymbolPreview.Draw(gr, new Rectangle(0, 0, bm.Width, bm.Height), symbol);
            gr.Dispose();
            gr = null;
            imageList1.Images.Add(bm);

            list.Items.Add(new SymbolListViewItem(symbol, labels, imageList1.Images.Count - 1, userObject));
        }

        public object UserObject
        {
            get
            {
                if (list.SelectedItems.Count != 1)
                    return null;

                return ((SymbolListViewItem)list.SelectedItems[0]).UserObject;
            }
        }

        public string ValueText
        {
            get
            {
                if (list.SelectedItems.Count == 1)
                {
                    return list.SelectedItems[0].Text;
                }
                return String.Empty;
            }
            set
            {
                if (list.SelectedItems.Count == 1)
                {
                    list.SelectedItems[0].Text = value;
                }
            }
        }
        public string LegendText
        {
            get
            {
                if (list.SelectedItems.Count == 1)
                {
                    return list.SelectedItems[0].SubItems[1].Text;
                }
                return String.Empty;
            }
            set
            {
                if (list.SelectedItems.Count == 1)
                {
                    list.SelectedItems[0].SubItems[1].Text = value;
                }
            }
        }

        public string[] OrderedKeys
        {
            get
            {
                string[] keys = new string[list.Items.Count];
                for (int i = 0; i < list.Items.Count; i++)
                {
                    keys[i] = list.Items[i].SubItems[0].Text;
                }
                return keys;
            }
        }

        public void RemoveSelected()
        {
            if (list.SelectedItems.Count > 0)
            {
                for (int i = list.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    list.Items.Remove(list.Items[list.SelectedIndices[i]]);
                }
            }
        }
        /// <summary> 
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SymbolsListView));
            this.list = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this._box = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // list
            // 
            resources.ApplyResources(this.list, "list");
            this.list.AllowDrop = true;
            this.list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.list.FullRowSelect = true;
            this.list.HideSelection = false;
            this.list.Name = "list";
            this.list.SmallImageList = this.imageList1;
            this.list.UseCompatibleStateImageBehavior = false;
            this.list.View = System.Windows.Forms.View.Details;
            this.list.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.list_ItemDrag);
            this.list.SelectedIndexChanged += new System.EventHandler(this.list_SelectedIndexChanged);
            this.list.DragDrop += new System.Windows.Forms.DragEventHandler(this.list_DragDrop);
            this.list.DragEnter += new System.Windows.Forms.DragEventHandler(this.list_DragEnter);
            this.list.DragOver += new System.Windows.Forms.DragEventHandler(this.list_DragOver);
            this.list.KeyDown += new System.Windows.Forms.KeyEventHandler(this.list_KeyDown);
            this.list.MouseDown += new System.Windows.Forms.MouseEventHandler(this.list_MouseDown);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            resources.ApplyResources(this.imageList1, "imageList1");
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _box
            // 
            resources.ApplyResources(this._box, "_box");
            this._box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._box.Name = "_box";
            this._box.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._box_KeyPress);
            this._box.Leave += new System.EventHandler(this._box_Leave);
            // 
            // SymbolsListView
            // 
            resources.ApplyResources(this, "$this");
            this.AllowDrop = true;
            this.Controls.Add(this._box);
            this.Controls.Add(this.list);
            this.Name = "SymbolsListView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public void Clear()
        {
            list.Items.Clear();
        }

        private int _labelEditIndex = 0;
        private SymbolListViewItem _labelEditItem = null;
        private void list_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem item = list.GetItemAt(4, e.Y);
            if (!(item is SymbolListViewItem)) return;

            if (e.X < imageList1.ImageSize.Width)
            {
                ISymbol symbol = ((SymbolListViewItem)item).Symbol;
                if (OnSymbolClicked != null)
                {
                    OnSymbolClicked(symbol);

                    Image image = imageList1.Images[item.ImageIndex];
                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(image))
                    {
                        SymbolPreview.Draw(gr, new Rectangle(0, 0, image.Width, image.Height), symbol, true);
                        imageList1.Images[item.ImageIndex] = image;
                    }
                    list.Refresh();
                }
                else
                {
                    ISymbol s = (symbol is INullSymbol) ? RendererFunctions.CreateStandardSymbol(((INullSymbol)symbol).GeomtryType) : (ISymbol)symbol.Clone();
                    FormSymbol dlg = new FormSymbol(s);

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Image image = imageList1.Images[item.ImageIndex];
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(image))
                        {
                            SymbolPreview.Draw(gr, new Rectangle(0, 0, image.Width, image.Height), dlg.Symbol, true);
                            imageList1.Images[item.ImageIndex] = image;

                            if (OnSymbolChanged != null) OnSymbolChanged(item.Text, dlg.Symbol);
                        }
                        ((SymbolListViewItem)item).Symbol = dlg.Symbol;
                    }
                }

                if (_box.Visible)
                    HideBox();
            }
            else
            {
                int w = 0;
                bool showBox = false;
                foreach (ColumnHeader col in list.Columns)
                {
                    w += col.Width;
                    if (e.X < w)
                    {
                        if (col.Index == 0 && OnKeyChanged == null) break;
                        if (col.Index > 0 && OnLabelChanged == null) break;

                        Rectangle rect = item.Bounds;
                        rect.X = 3 + w - col.Width + ((col.Index == 0) ? imageList1.ImageSize.Width : 0);
                        rect.Y = 3 + rect.Top + rect.Height / 2 - _box.Height / 2;
                        rect.Width = col.Width - ((col.Index == 0) ? imageList1.ImageSize.Width : 0);
                        rect.Height = _box.Height;

                        if (_box.Visible == true)
                            HideBox();

                        _box.Bounds = rect;
                        _box.Text = item.SubItems[col.Index].Text;
                        _box.Visible = true;
                        _box.Focus();
                        _labelEditItem = (SymbolListViewItem)item;
                        _labelEditIndex = col.Index;
                        showBox = true;
                        break;
                    }

                }
                if (showBox == false && _box.Visible == true)
                {
                    HideBox();
                    _labelEditIndex = 0;
                }
            }
        }

        private void _box_Leave(object sender, EventArgs e)
        {
            //HideBox();
        }

        private void _box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                HideBox();
            }
        }

        private void HideBox()
        {
            _box.Visible = false;

            lock (lockThis)
            {
                if (_labelEditItem != null)
                {
                    if (_labelEditIndex == 0 && OnKeyChanged != null)
                    {
                        // Validierung, ob key eindeutig ist fehlt noch!!
                        OnKeyChanged(_labelEditItem.SubItems[0].Text, _box.Text);
                        _labelEditItem.SubItems[0].Text = _box.Text;
                    }
                    else if (_labelEditIndex > 0 && OnLabelChanged != null)
                    {
                        OnLabelChanged(_labelEditItem.Symbol, _labelEditIndex, _box.Text);
                        _labelEditItem.SubItems[_labelEditIndex].Text = _box.Text;
                    }
                    _labelEditItem = null;
                }
            }
            _labelEditIndex = 0;
        }
        private void list_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 46 && OnDeleteItem != null)
            {
                foreach (ListViewItem item in list.SelectedItems)
                {
                    OnDeleteItem(item.Text);
                    list.Items.Remove(item);
                }
            }
        }

        #region Item Classes
        public class SymbolListViewItem : ListViewItem
        {
            private ISymbol _symbol;
            private object _userObject = null;

            public SymbolListViewItem(ISymbol symbol, string[] items, int imageIndex)
                : base(items, imageIndex)
            {
                _symbol = symbol;
            }
            public SymbolListViewItem(ISymbol symbol, string[] items, int imageList, object userObject)
                : this(symbol, items, imageList)
            {
                _userObject = userObject;
            }

            public ISymbol Symbol
            {
                get { return _symbol; }
                set { _symbol = value; }
            }

            public object UserObject
            {
                get { return _userObject; }
                set { _userObject = value; }
            }
        }
        #endregion

        #region Drag & Drop
        private DragEventData dragData = null;
        private void list_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (dragData == null)
                    return;

                ListViewItem[] sel = null;
                if (e.Effect == DragDropEffects.Move &&
                    dragData.DragToItem != null &&
                    list.SelectedItems.Count != 0)
                {
                    //Obtain the index of the item at the mouse pointer.
                    sel = new ListViewItem[list.SelectedItems.Count];
                    for (int i = 0; i <= list.SelectedItems.Count - 1; i++)
                    {
                        sel[i] = list.SelectedItems[i];
                    }
                }

                if (sel == null) return;

                int counter = 0;
                foreach (ListViewItem selItem in sel)
                {
                    if (selItem == dragData.DragToItem) continue;

                    if (list.Items.Contains(selItem))
                        list.Items.Remove(selItem);

                    int dragIndex = dragData.DragToItem.Index;
                    switch (dragData.Mode)
                    {
                        case DragEventData.DragEventTargetMode.insertBefore:
                            list.Items.Insert(dragIndex, selItem);
                            break;
                        case DragEventData.DragEventTargetMode.insertAfter:
                            list.Items.Insert(dragIndex + 1 + counter++, selItem);
                            break;
                    }
                }
                if (AfterLegendOrdering != null)
                    AfterLegendOrdering(this, new EventArgs());
            }
            finally
            {
                if (dragData != null && dragData.DragToItem != null)
                {
                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(list.Handle))
                    {
                        DragEventData.DrawMoveLine(
                            gr, list.BackColor, dragData.DragToItem.Bounds, dragData.Mode);
                    }
                }
                dragData = null;
            }
        }

        private void list_DragEnter(object sender, DragEventArgs e)
        {
            dragData = null;
            int len = e.Data.GetFormats().Length - 1;
            for (int i = 0; i <= len; i++)
            {
                if (e.Data.GetFormats()[i].Equals("System.Windows.Forms.ListView+SelectedListViewItemCollection"))
                {
                    //The data from the drag source is moved to the target.	
                    e.Effect = DragDropEffects.Move;
                    dragData = new DragEventData();
                    return;
                }
            }
        }

        private void list_DragOver(object sender, DragEventArgs e)
        {
            if (dragData == null) return;

            Point cp = list.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = list.GetItemAt(10, cp.Y);
            if (dragToItem == null)
            {
                return;
            }

            DragEventData.DragEventTargetMode mode =
                ((cp.Y > (dragToItem.Bounds.Top + dragToItem.Bounds.Height / 2)) ?
                    DragEventData.DragEventTargetMode.insertAfter :
                    DragEventData.DragEventTargetMode.insertBefore);
            if (dragData.DragToItem == dragToItem &&
                dragData.Mode == mode) return;

            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(list.Handle))
            {
                if (dragData.DragToItem != null)
                    DragEventData.DrawMoveLine(gr, list.BackColor, dragData.DragToItem.Bounds, dragData.Mode);
                DragEventData.DrawMoveLine(gr, Color.Red, dragToItem.Bounds, mode);
            }
            dragData.DragToItem = dragToItem;
            dragData.Mode = mode;
        }
        private void list_ItemDrag(object sender, ItemDragEventArgs e)
        {
            //Begins a drag-and-drop operation in the ListView control.
            list.DoDragDrop(list.SelectedItems, DragDropEffects.Move);
        }

        private class DragEventData
        {
            public enum DragEventTargetMode { ignore = 0, insertBefore = 1, insertAfter = 2, insertInto = 3 }
            public ListViewItem DragToItem = null;
            public DragEventTargetMode Mode = DragEventTargetMode.ignore;

            public static void DrawMoveLine(System.Drawing.Graphics gr, Color col, Rectangle rect, DragEventTargetMode mode)
            {
                using (Pen pen = new Pen(col, 2))
                {
                    if (mode == DragEventTargetMode.insertBefore)
                    {
                        gr.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);

                        gr.DrawLine(pen, rect.Left + 2, rect.Top - 1, rect.Left, rect.Top - 3);
                        gr.DrawLine(pen, rect.Left + 2, rect.Top, rect.Left, rect.Top + 2);

                        gr.DrawLine(pen, rect.Right - 2, rect.Top - 1, rect.Right, rect.Top - 3);
                        gr.DrawLine(pen, rect.Right - 2, rect.Top, rect.Right, rect.Top + 2);
                    }
                    else if (mode == DragEventTargetMode.insertAfter)
                    {
                        gr.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);

                        gr.DrawLine(pen, rect.Left + 2, rect.Bottom - 1, rect.Left, rect.Bottom - 3);
                        gr.DrawLine(pen, rect.Left + 2, rect.Bottom, rect.Left, rect.Bottom + 2);

                        gr.DrawLine(pen, rect.Right - 2, rect.Bottom - 1, rect.Right, rect.Bottom - 3);
                        gr.DrawLine(pen, rect.Right - 2, rect.Bottom, rect.Right, rect.Bottom + 2);
                    }
                    else if (mode == DragEventTargetMode.insertInto)
                    {
                        gr.DrawRectangle(pen, rect);
                    }
                }
            }
        }
        #endregion

        private void list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, e);
        }
        public ListView.SelectedListViewItemCollection SelectedItems
        {
            get
            {
                return list.SelectedItems;
            }
        }
        public ListView.ListViewItemCollection Items
        {
            get
            {
                return list.Items;
            }
        }
    }
}
