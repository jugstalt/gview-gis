using gView.Desktop.Wpf.Controls;
using gView.Framework.UI;
using System;
using System.Windows;

namespace gView.Win.Carto.Items
{
    internal class DropDownToolButton : Fluent.DropDownButton, ICheckAbleButton
    {
        private IToolMenu _tool;
        private bool _checked = false;
        private object _contextObject = null;

        public DropDownToolButton(IToolMenu tool)
            : this(tool, null)
        {
        }

        public DropDownToolButton(IToolMenu tool, object contextObject)
        {
            _tool = tool;
            _contextObject = contextObject;

            if (_tool == null || _tool.DropDownTools == null) return;

            if (_tool != null && _tool.SelectedTool != null)
            {
                base.Icon = base.LargeIcon = ImageFactory.FromBitmap(_tool.SelectedTool.Image as System.Drawing.Image);
                base.Header = _tool.SelectedTool.Name;
            }

            foreach (ITool t in _tool.DropDownTools)
            {
                DropDownToolButtonItem item = new DropDownToolButtonItem(this, t);
                item.Click += new RoutedEventHandler(button_Click);
                base.Items.Add(item);
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (!(sender is DropDownToolButtonItem)) return;

            _tool.SelectedTool = ((DropDownToolButtonItem)sender).Tool;
            base.Icon = base.LargeIcon = ImageFactory.FromBitmap(_tool.SelectedTool.Image as System.Drawing.Image);
            base.Header = _tool.SelectedTool.Name;

            this.OnClick();
        }

        public ITool Tool
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

        #endregion ICheckAbleButton Member

        #region Events

        public event RoutedEventHandler Click = null;

        public void OnClick()
        {
            if (Click != null)
                Click(this, new RoutedEventArgs());
            else if (_contextObject != null && _tool.SelectedTool != null)
                _tool.SelectedTool.OnEvent(_contextObject);
        }

        #endregion Events
    }

    internal class DropDownToolButtonItem : Fluent.Button
    {
        private ITool _tool;
        private DropDownToolButton _parent;

        public DropDownToolButtonItem(DropDownToolButton parent, ITool tool)
        {
            _parent = parent;
            _tool = tool;

            base.Icon = base.LargeIcon = ImageFactory.FromBitmap(tool.Image as System.Drawing.Image);
            base.Header = tool.Name;
            base.SizeDefinition = "Middle";
        }

        public ITool Tool
        {
            get { return _tool; }
        }
    }
}