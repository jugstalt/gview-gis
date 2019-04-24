using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.Network;
using System.Xml;
using gView.Framework.Globalisation;
using System.Threading.Tasks;

namespace gView.Plugins.Network
{
    [RegisterPlugIn("17475DC9-5A9B-4c90-8DE1-60654389F108")]
    public class NetworkTracerSelector : ITool, IToolItem
    {
        private IMapDocument _doc = null;
        private ToolStripComboBox _combo;
        private Module _module = null;

        public NetworkTracerSelector()
        {
            _combo = new ToolStripComboBox();
            _combo.DropDownStyle = ComboBoxStyle.DropDownList;
            _combo.Width = 500;
            _combo.SelectedIndexChanged += new EventHandler(_combo_SelectedIndexChanged);
        }

        void _combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_module != null)
            {
                if (_combo.SelectedItem is NetworkTracerItem)
                    _module.SelectedNetworkTracer = ((NetworkTracerItem)_combo.SelectedItem).NetworkTracer;
                else
                    _module.SelectedNetworkTracer = null;

                if (_doc != null && _doc.Application is IGUIApplication)
                    ((IGUIApplication)_doc.Application).ValidateUI();
            }
        }

        #region ITool Member

        public string Name
        {
            get { return "Network Tracer Selector"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                _module = Module.GetModule(_doc);

                PlugInManager pluginMan = new PlugInManager();
                foreach (var tracerType in pluginMan.GetPlugins(gView.Framework.system.Plugins.Type.INetworkTracer))
                {
                    INetworkTracer tracer = pluginMan.CreateInstance<INetworkTracer>(tracerType);
                    if (tracer == null)
                        continue;

                    _combo.Items.Add(new NetworkTracerItem(tracer));
                }
                if (_combo.Items.Count > 0)
                    _combo.SelectedIndex = 0;
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Member

        public ToolStripItem ToolItem
        {
            get { return _combo; }
        }

        #endregion

        #region ItemClasses
        private class NetworkTracerItem
        {
            private INetworkTracer _tracer;

            public NetworkTracerItem(INetworkTracer tracer)
            {
                _tracer = tracer;
            }

            public INetworkTracer NetworkTracer
            {
                get { return _tracer; }
            }

            public override string ToString()
            {
                string ret = _tracer.Name;
                if (ret.StartsWith("Trace "))
                    ret = ret.Substring(6, ret.Length - 6);

                return ret;
            }
        }
        #endregion
    }

    [RegisterPlugIn("83A38411-27C7-4241-9F28-AC3005BFAFA8")]
    public class NetworkTracerSelectorLabel : ITool, IToolItem
    {
        #region ITool Member

        public string Name
        {
            get { return "Networktracer Selector Label"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Member

        public ToolStripItem ToolItem
        {
            get
            {
                return new ToolStripLabel(LocalizedResources.GetResString("String.NetworkTracer", "Trace"));
            }
        }

        #endregion
    }
}
