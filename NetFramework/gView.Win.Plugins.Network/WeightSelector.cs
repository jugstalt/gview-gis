using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.UI;
using System.Windows.Forms;
using gView.Framework.Globalisation;
using gView.Framework.Network;
using System.Threading.Tasks;

namespace gView.Plugins.Network
{
    [RegisterPlugInAttribute("9A975F46-D727-495b-B752-9E079289E296")]
    public class WeightSelector : ITool, IToolItem
    {
        private IMapDocument _doc = null;
        private ToolStripMenuItem _menu, _multiWeight, _trueCost, _weights;
        private Module _module = null;

        public WeightSelector()
        {
            _menu = new ToolStripMenuItem(LocalizedResources.GetResString("String.NetworkWeights", "Weights"));

            _weights = new ToolStripMenuItem("Use Weight");
            _menu.DropDownItems.Add(_weights);
            _multiWeight = new ToolStripMenuItem("Use as weight");
            _trueCost = new ToolStripMenuItem("Use as actual costs");

            _multiWeight.Click += new EventHandler(_multiWeight_Click);
            _trueCost.Click += new EventHandler(_trueCost_Click);

            _menu.DropDownItems.Add(_multiWeight);
            _menu.DropDownItems.Add(_trueCost);
        }

        #region ITool Member

        public string Name
        {
            get { return "Weights"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return "Weights"; }
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

                if (_module != null)
                {
                    _module.OnSelectedNetorkFeatureClassChanged += new EventHandler(_module_OnSelectedNetorkFeatureClassChanged);
                    switch (_module.WeightApplying)
                    {
                        case WeightApplying.Weight:
                            _multiWeight.Checked = true;
                            break;
                        case WeightApplying.ActualCosts:
                            _trueCost.Checked = true;
                            break;
                    }
                }
            }
        }
        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Member

        public System.Windows.Forms.ToolStripItem ToolItem
        {
            get { return _menu; }
        }

        #endregion

        #region Members
        public void FillWeightsMenu()
        {
            _weights.DropDownItems.Clear();

            _weights.DropDownItems.Add(new WeightToolStripMenuItem(null, true));
            if (_module != null && _module.SelectedNetworkFeatureClass != null &&
                _module.SelectedNetworkFeatureClass.GraphWeights != null)
            {
                foreach (IGraphWeight weight in _module.SelectedNetworkFeatureClass.GraphWeights)
                {
                    _weights.DropDownItems.Add(new WeightToolStripMenuItem(weight));
                }
            }

            foreach (WeightToolStripMenuItem item in _weights.DropDownItems)
            {
                item.Click += new EventHandler(item_Click);
            }
        }


        #endregion

        #region Item Classes
        private class WeightToolStripMenuItem : ToolStripMenuItem
        {
            IGraphWeight _weight;

            public WeightToolStripMenuItem(IGraphWeight weight)
            {
                _weight = weight;
                if (_weight == null)
                    base.Text = LocalizedResources.GetResString("String.none", "none");
                else
                    base.Text = _weight.Name;
            }
            public WeightToolStripMenuItem(IGraphWeight weight, bool check)
                : this(weight)
            {
                base.Checked = true;
            }

            public IGraphWeight GraphWeight
            {
                get { return _weight; }
            }
        }
        #endregion

        #region Events
        void _trueCost_Click(object sender, EventArgs e)
        {
            _trueCost.Checked = true;
            _multiWeight.Checked = false;

            if (_module != null)
                _module.WeightApplying = WeightApplying.ActualCosts;
        }

        void _multiWeight_Click(object sender, EventArgs e)
        {
            _multiWeight.Checked = true;
            _trueCost.Checked = false;

            if (_module != null)
                _module.WeightApplying = WeightApplying.Weight;
        }

        void _module_OnSelectedNetorkFeatureClassChanged(object sender, EventArgs e)
        {
            FillWeightsMenu();
        }

        void item_Click(object sender, EventArgs e)
        {
            if(!(sender is WeightToolStripMenuItem))
                return;
            foreach (WeightToolStripMenuItem item in _weights.DropDownItems)
                item.Checked = false;
            ((WeightToolStripMenuItem)sender).Checked = true;

            if (_module != null)
            {
                _module.GraphWeight = ((WeightToolStripMenuItem)sender).GraphWeight;
            }
        }
        #endregion
    }
}
