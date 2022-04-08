using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Windows.Forms;

namespace gView.Plugins.Editor.Dialogs
{
    public partial class FormDomains : Form
    {
        IFieldDomain _domain = null;

        public FormDomains(IFieldDomain domain)
        {
            InitializeComponent();

            _domain = domain;
        }

        private void FormDomains_Load(object sender, EventArgs e)
        {
            PlugInManager compMan = new PlugInManager();

            cmbDomains.Items.Add(new DomainItem(null));
            cmbDomains.SelectedIndex = 0;

            foreach (var domainType in compMan.GetPlugins(gView.Framework.system.Plugins.Type.IFieldDomain))
            {
                IFieldDomain fDomain = compMan.CreateInstance<IFieldDomain>(domainType);
                if (fDomain == null)
                {
                    continue;
                }

                if (_domain != null && fDomain.GetType().Equals(_domain.GetType()))
                {
                    cmbDomains.Items.Add(new DomainItem(_domain));
                    cmbDomains.SelectedIndex = cmbDomains.Items.Count - 1;
                }
                else
                {
                    cmbDomains.Items.Add(new DomainItem(fDomain));
                }
            }
        }

        public IFieldDomain Domain
        {
            get { return ((DomainItem)cmbDomains.SelectedItem).FieldDomain; }
        }

        #region ItemClasses
        private class DomainItem
        {
            private IFieldDomain _domain;

            public DomainItem(IFieldDomain domain)
            {
                _domain = domain;
            }

            public IFieldDomain FieldDomain
            {
                get { return _domain; }
            }

            public override string ToString()
            {
                if (_domain == null)
                {
                    return "none";
                }

                return _domain.Name;
            }
        }
        #endregion

        private void cmbDomains_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupBoxControl.Controls.Clear();

            DomainItem item = cmbDomains.SelectedItem as DomainItem;
            if (item == null)
            {
                return;
            }

            if (item.FieldDomain != null &&
                item.FieldDomain is IPropertyPage)
            {
                Control ctrl = ((IPropertyPage)item.FieldDomain).PropertyPage(item.FieldDomain) as Control;
                if (ctrl != null)
                {
                    groupBoxControl.Controls.Add(ctrl);
                    ctrl.Dock = DockStyle.Fill;
                }
            }
        }
    }
}