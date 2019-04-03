using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Framework.UI.Controls
{
    internal enum ToolbarPanel { top = 0, bottom = 1, left = 2, right = 3, flow = 4 }
    public class ToolbarStrip : ToolStrip, IToolbar,IPlugInWrapper
    {
        IToolbar _toolbar;
        ToolStripContainer _container;
        ToolbarPanel _panel = ToolbarPanel.top;

        public ToolbarStrip(IToolbar toolbar, ToolStripContainer container)
        {
            _toolbar = toolbar;
            _container = container;

            base.ParentChanged += new EventHandler(base_ParentChanged);
        }

        public IToolbar Toolbar
        {
            get { return _toolbar; }
        }

        private void base_ParentChanged(object sender, EventArgs e)
        {
            if (sender is ToolbarStrip)
            {
                if (((ToolbarStrip)sender).Parent == ToolPanel(ToolbarPanel.top))
                {
                    _panel = ToolbarPanel.top;
                }
                else if (((ToolbarStrip)sender).Parent == ToolPanel(ToolbarPanel.bottom))
                {
                    _panel = ToolbarPanel.bottom;
                }
                else if (((ToolbarStrip)sender).Parent == ToolPanel(ToolbarPanel.left))
                {
                    _panel = ToolbarPanel.left;
                }
                else if (((ToolbarStrip)sender).Parent == ToolPanel(ToolbarPanel.right))
                {
                    _panel = ToolbarPanel.right;
                }
            }
        }

        private ToolStripPanel ToolPanel(ToolbarPanel panel)
        {
            if (_container == null) return null;

            switch (panel)
            {
                case ToolbarPanel.top:
                    return _container.TopToolStripPanel;
                case ToolbarPanel.bottom:
                    return _container.BottomToolStripPanel;
                case ToolbarPanel.left:
                    return _container.LeftToolStripPanel;
                case ToolbarPanel.right:
                    return _container.RightToolStripPanel;
            }
            return null;
        }

        #region IToolbar Member

        public List<Guid> GUIDs
        {
            get
            {
                if (_toolbar == null) return new List<Guid>();
                return _toolbar.GUIDs;
            }
            set
            {
                if (_toolbar != null) _toolbar.GUIDs = value;
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_toolbar == null) return;
            _toolbar.Load(stream);

            this.Visible = (bool)stream.Load("visible", true);
            _panel = (ToolbarPanel)stream.Load("panel", (int)ToolbarPanel.top);

            int x = (int)stream.Load("x", 0);
            int y = (int)stream.Load("y", 0);

            if (x >= 0 && y >= 0)
            {
                if (!this.Visible && x == 0 && y == 0)
                {
                    y += 350;
                }
                this.Location = new System.Drawing.Point(x, y);
            }
            ToolStripPanel toolStripPanel = this.ToolPanel(_panel);
            if (toolStripPanel != null && this.Parent != toolStripPanel)
            {
                if (this.Parent != null) this.Parent.Controls.Remove(this);
                toolStripPanel.Controls.Add(this);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_toolbar == null) return;
            _toolbar.Save(stream);

            stream.Save("visible", this.Visible);
            stream.Save("panel", (int)_panel);
            stream.Save("x", this.Location.X);
            stream.Save("y", this.Location.Y);
        }

        #endregion

        #region IPlugInWrapper Member

        public object WrappedPlugIn
        {
            get { return _toolbar; }
        }

        #endregion
    }
}
