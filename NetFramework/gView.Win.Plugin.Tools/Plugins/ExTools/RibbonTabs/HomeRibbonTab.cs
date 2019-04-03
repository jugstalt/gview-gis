using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;

namespace gView.Plugins.ExTools.RibbonTabs
{
    [gView.Framework.system.RegisterPlugIn("8cbb363b-6d7f-4514-904b-034dc615e662")]
    public class HomeRibbonTab : IExplorerRibbonTab
    {
        private List<RibbonGroupBox> _groups;

        public HomeRibbonTab()
        {
            _groups = new List<RibbonGroupBox>(
                new RibbonGroupBox[]{
                    new RibbonGroupBox("Clipboard",
                        new RibbonItem[] {
                            new RibbonItem(new Guid("97F70651-C96C-4b6e-A6AD-E9D3A22BFD45")),  // Copy
                            new RibbonItem(new Guid("18fed44c-a8e5-4a80-ad0c-32b9acc21a05")),  // CopyTo
                            new RibbonItem(new Guid("0F8673B3-F1C9-4f5f-86C4-2B41FCBE535B")),  // Paste
                            new RibbonItem(new Guid("3E36C2AD-2C58-42ca-A662-EE0C2DC1369D"), "Middle"),  // Cut
                            new RibbonItem(new Guid("80A04810-62D1-4DD9-94EF-8F67C30CC44B"), "Middle")   // Paste Schema
                        }
                        ),
                   new RibbonGroupBox("Organize",
                            new RibbonItem[] {
                            new RibbonItem(new Guid("4F54D455-1C22-469e-9DBB-78DBBEF6078D"), "Middle"),  // Delete
                            new RibbonItem(new Guid("91F17A8E-D859-4840-A287-7FDE14152CB1"), "Middle")   // Rename
                        }
                        ),
                    new RibbonGroupBox("",
                            new RibbonItem[] {
                            new RibbonItem(new Guid("54597B19-B37A-4fa2-BA45-8B4137F2910E")),  // Refresh
                            new RibbonItem(new Guid("7cbbed5e-c071-46de-b30a-9c6140dafd75"))   // Add Network Directory
                        }
                        )
                }
                );
        }

        #region IExplorerRibbonTab Member

        public string Header
        {
            get { return "Home"; }
        }

        public List<RibbonGroupBox> Groups
        {
            get
            {
                return _groups;
            }
        }

        public int Order
        {
            get { return 0; }
        }

        #endregion
    }
}
