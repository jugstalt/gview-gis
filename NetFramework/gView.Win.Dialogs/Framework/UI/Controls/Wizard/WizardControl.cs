using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.UI.Controls.Wizard
{
    public partial class WizardControl : UserControl
    {
        private List<Control> _pages = new List<Control>();
        private int _pageIndex = 0;

        public WizardControl()
        {
            InitializeComponent();
        }

        public void AddPage(Control control)
        {
            if (control != null && !_pages.Contains(control))
            {
                control.Dock = DockStyle.Fill;
                _pages.Add(control);
            }
        }

        public void Init()
        {
            RefreshPage();
        }

        private void RefreshPage()
        {
            if (_pageIndex <= PageIndexMax)
            {
                panelPage.Controls.Clear();
                if (_pages[_pageIndex] is IWizardPageNotification)
                    ((IWizardPageNotification)_pages[_pageIndex]).OnShowWizardPage();

                panelPage.Controls.Add(_pages[_pageIndex]);
            }

            btnPrev.Enabled = (_pageIndex > 0);

            btnNext.DialogResult = (_pageIndex == (PageIndexMax) ? DialogResult.OK : DialogResult.None);
            btnNext.Text = (_pageIndex == (PageIndexMax) ? "OK" : "Next >");
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_pageIndex <= PageIndexMax)
            {
                _pageIndex++;
                if (CheckPageNecessity(_pageIndex) == false)
                    btnNext_Click(sender, e);
                else
                    RefreshPage();
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (_pageIndex > 0)
            {
                _pageIndex--;
                if (CheckPageNecessity(_pageIndex) == false)
                    btnPrev_Click(sender, e);
                else
                    RefreshPage();
            }
        }

        private bool CheckPageNecessity(int pageIndex)
        {
            if (pageIndex < _pages.Count)
            {
                if (_pages[pageIndex] is IWizardPageNecessity &&
                    ((IWizardPageNecessity)_pages[pageIndex]).CheckNecessity() == false)
                    return false;

                return true;
            }
            return false;
        }

        private int PageIndexMax
        {
            get
            {
                int max = 0;
                for (int i = 0; i < _pages.Count; i++)
                {
                    if (CheckPageNecessity(i))
                        max = i;
                }
                return max;
            }
        }
    }
}
