using gView.Framework.system;
using gView.Framework.UI;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gView.Plugins.MapTools.Controls
{
    public partial class MapOptionsControl : UserControl, IControl
    {
        IMapDocument _document = null;
        List<IMapOptionPage> _optionPages = new List<IMapOptionPage>();

        public MapOptionsControl()
        {
            InitializeComponent();
        }

        private void Commit()
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

        #region IControl Member

        public void OnShowControl(object hook)
        {
            if (hook is IMapDocument)
            {
                tabControl.TabPages.Clear();
                _document = (IMapDocument)hook;

                PlugInManager compMan = new PlugInManager();
                foreach (var pageType in compMan.GetPlugins(gView.Framework.system.Plugins.Type.IMapOptionPage))
                {
                    IMapOptionPage page = compMan.CreateInstance<IMapOptionPage>(pageType);
                    if (page == null)
                    {
                        continue;
                    }

                    Panel pagePanel = page.OptionPage(_document);
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
        }

        public void UnloadControl()
        {
            Commit();
        }

        #endregion
    }
}
