using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using gView.Desktop.Wpf.Controls;
using System.Windows;
using System.Windows.Media;

namespace gView.Win.DataExplorer.Items
{
    internal class DropDownToolButton : Fluent.DropDownButton
    {
        private IExToolMenu _tool;
        private bool _checked = false;

        public DropDownToolButton(IExToolMenu tool)
        {
            _tool = tool;
            if (_tool == null || _tool.DropDownTools == null) return;

            if (_tool != null && _tool.SelectedTool != null)
            {
                base.Icon = base.LargeIcon = ImageFactory.FromBitmap(_tool.SelectedTool.Image as global::System.Drawing.Image);
                base.Header = _tool.SelectedTool.Name;
            }

            foreach (IExTool t in _tool.DropDownTools)
            {
                DropDownToolButtonItem item = new DropDownToolButtonItem(this, t);
                item.Click += new RoutedEventHandler(button_Click);
                base.Items.Add(item);
            }
        }

        void button_Click(object sender, EventArgs e)
        {
            if (!(sender is DropDownToolButtonItem)) return;

            _tool.SelectedTool = ((DropDownToolButtonItem)sender).Tool;
            base.Icon = base.LargeIcon= ImageFactory.FromBitmap(_tool.SelectedTool.Image as global::System.Drawing.Image);
            base.Header = _tool.SelectedTool.Name;

            this.OnClick();
        }

        public IExTool Tool
        {
            get { return _tool.SelectedTool; }
        }

        #region ICheckAbleButton Member

        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                _checked = value;
            }
        }

        #endregion

        #region Events

        public event RoutedEventHandler Click = null;

        public void OnClick()
        {
            if (Click != null)
                Click(this, new RoutedEventArgs());
        }

        #endregion
    }

    internal class DropDownToolButtonItem : Fluent.Button
    {
        private IExTool _tool;
        private DropDownToolButton _parent;

        public DropDownToolButtonItem(DropDownToolButton parent, IExTool tool)
        {
            _parent = parent;
            _tool = tool;

            base.Icon = base.LargeIcon= ImageFactory.FromBitmap(tool.Image as global::System.Drawing.Image);
            base.Header = tool.Name;
            base.SizeDefinition = "Middle";
        }

        public IExTool Tool
        {
            get { return _tool; }
        }
    }
}
