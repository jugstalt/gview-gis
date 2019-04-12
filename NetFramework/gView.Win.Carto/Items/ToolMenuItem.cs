using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using gView.Desktop.Wpf.Controls;

namespace gView.Win.Carto.Items
{
    internal class ToolMenuItem : System.Windows.Controls.MenuItem
    {
        private ITool _tool;

        public ToolMenuItem(ITool tool)
        {
            _tool = tool;

            base.Icon = ImageFactory.FromBitmap(tool.Image as System.Drawing.Image);
            base.Header = tool.Name;

            /*
            if (_tool is IShortCut)
            {
                base.ShortcutKeys = ((IShortCut)_tool).ShortcutKeys;
                base.ShortcutKeyDisplayString = ((IShortCut)_tool).ShortcutKeyDisplayString;
                base.ShowShortcutKeys = true;
            }
             * */
        }

        public ITool Tool
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
