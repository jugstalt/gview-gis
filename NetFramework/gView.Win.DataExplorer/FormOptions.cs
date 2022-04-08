using gView.Framework.UI;
using System;
using System.Windows.Forms;

namespace gView.Win.DataExplorer
{
    public partial class FormOptions : Form
    {
        private IExplorerOptionPage _page;

        public FormOptions(IExplorerOptionPage page)
        {
            _page = page;

            InitializeComponent();

            Panel panel = _page.OptionPage();
            if (panel != null)
            {
                this.Width = panel.Width + 10;
                this.Height = panel.Height + 100;
                panelContainer.Controls.Add(panel);

                this.Text += ": " + page.Title;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_page != null)
            {
                _page.Commit();
            }
        }
    }
}
