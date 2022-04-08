using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormExplorerOptions : Form
    {
        IMapDocument _document = null;
        List<IExplorerOptionPage> _optionPages = new List<IExplorerOptionPage>();

        public FormExplorerOptions(IMapDocument document)
        {
            InitializeComponent();

            _document = document;

            PlugInManager compMan = new PlugInManager();
            foreach (var pageTypes in compMan.GetPlugins(Plugins.Type.IExplorerOptionPage))
            {
                IExplorerOptionPage page = compMan.CreateInstance<IExplorerOptionPage>(pageTypes);
                if (page == null)
                {
                    continue;
                }

                Panel pagePanel = page.OptionPage();
                if (pagePanel == null)
                {
                    continue;
                }

                TabPage tabPage = new TabPage(page.Title);
                tabPage.Controls.Add(pagePanel);

                if (page.Image != null)
                {
                    imageList1.Images.Add(page.Image);
                    tabPage.ImageIndex = imageList1.Images.Count - 1;
                }

                tabControl.TabPages.Add(tabPage);
                _optionPages.Add(page);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            foreach (IMapOptionPage page in _optionPages)
            {
                if (page == null)
                {
                    continue;
                }

                page.Commit();
            }
        }
    }
}