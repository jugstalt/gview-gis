using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.IO;
using gView.Framework.Data;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormMetadata : Form
    {
        private Metadata _metadata;
        private object _metadataObject;

        public FormMetadata(XmlStream xmlStream, object metadataObject)
        {
            InitializeComponent();

            _metadata = new Metadata();
            _metadata.ReadMetadata(xmlStream);

            _metadataObject = metadataObject;
        }

        async private void FormMetadata_Load(object sender, EventArgs e)
        {
            if (await _metadata.GetProviders() == null) return;

            foreach (IMetadataProvider provider in await _metadata.GetProviders())
            {
                if (provider == null) continue;
                TabPage page = new TabPage(provider.Name);

                if (provider is IPropertyPage)
                {
                    Control ctrl = ((IPropertyPage)provider).PropertyPage(null) as Control;
                    if (ctrl != null)
                    {
                        page.Controls.Add(ctrl);
                        ctrl.Dock = DockStyle.Fill;
                    }
                    if (ctrl is IMetadataObjectParameter)
                        ((IMetadataObjectParameter)ctrl).MetadataObject = _metadataObject;
                }
                tabControl1.TabPages.Add(page);
            }
        }

        async public Task<XmlStream> GetStream()
        {
            XmlStream stream = new XmlStream("Metadata");
            if (_metadata != null)
                await _metadata.WriteMetadata(stream);

            return stream;
        }
    }
}