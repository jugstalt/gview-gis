using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Desktop.Wpf.Controls;
using gView.Framework.UI;

namespace gView.Desktop.Wpf.Items
{
    public class WpfToolMenuItem : System.Windows.Controls.MenuItem
    {
        private IExTool _tool;

        public WpfToolMenuItem(IExTool tool)
            : base()
        {
            _tool = tool;

            base.Header = tool.Name;
            base.Icon = ImageFactory.FromBitmap((System.Drawing.Image)tool.Image);

            //if (_tool is IShortCut)
            //{
            //    base.ShortcutKeys = ((IShortCut)_tool).ShortcutKeys;
            //    base.ShortcutKeyDisplayString = ((IShortCut)_tool).ShortcutKeyDisplayString;
            //    base.ShowShortcutKeys = true;
            //}
        }

        public IExTool Tool
        {
            get { return _tool; }
        }

        public bool Enabled
        {
            get
            {
                if (_tool != null) return _tool.Enabled;
                return base.IsEnabled;
            }
            set
            {
                base.IsEnabled = value;
            }
        }
    }
}
