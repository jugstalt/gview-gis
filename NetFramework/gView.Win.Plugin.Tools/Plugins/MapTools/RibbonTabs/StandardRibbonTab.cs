using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using gView.Plugins.MapTools.Dialogs;
using gView.Framework.system.UI;

namespace gView.Plugins.Tools.RibbonTabs
{
    [gView.Framework.system.RegisterPlugIn("88389436-c5fa-4171-9177-c14437f495fb")]
    public class StandardRibbonTab : ICartoRibbonTab
    {
        private List<RibbonGroupBox> _groups;

        public StandardRibbonTab()
        {
            _groups = new List<RibbonGroupBox>(
                new RibbonGroupBox[]{
                    new RibbonGroupBox("Navigation",
                        new RibbonItem[] {
                            new RibbonItem(new Guid("3E2E9F8C-24FB-48f6-B80E-1B0A54E8C309")),  // SmartNavigation
                            new RibbonItem(new Guid("09007AFA-B255-4864-AC4F-965DF330BFC4"), "Middle"),  // Zoomin Dyn
                            new RibbonItem(new Guid("51D04E6F-A13E-40b6-BF28-1B8E7C24493D"), "Middle"),  // Zoomout Dyn
                            new RibbonItem(new Guid("2680F0FD-31EE-48c1-B0F7-6674BAD0A688"), "Middle")   // Pan
                        }
                        ),
                    new RibbonGroupBox("Map Extent",
                        new RibbonItem[]{
                            new RibbonItem(new Guid("6351BCBE-809A-43cb-81AA-6414ED3FA459"),"Middle"),  // Zoomin Static
                            new RibbonItem(new Guid("E1C01E9D-8ADC-477b-BCD1-6B7BBA756D44"),"Middle"),  // Zoomout Static
                            new RibbonItem(new Guid("58AE3C1D-40CD-4f61-8C5C-0A955C010CF4"),"Middle"),  // Zoom To Full Extent
                            new RibbonItem(new Guid("00000000-0000-0000-0000-000000000000")),
                            new RibbonItem(new Guid("82F8E9C3-7B75-4633-AB7C-8F9637C2073D"),"Middle"),  // Back
                            new RibbonItem(new Guid("CFE66CDF-CD95-463c-8CD1-2541574D719A"),"Middle")   // Forward
                        }
                        ),
                    new RibbonGroupBox("Info", 
                        new RibbonItem[]{
                            //new RibbonItem(new Guid("1E21835C-FD41-4e68-8462-9FAA66EA5A54")),  // QueryTuemeText
                            new RibbonItem(new Guid("51A2CF81-E343-4c58-9A42-9207C8DFBC01")),  // QueryThemeCombo
                            new RibbonItem(new Guid("F13D5923-70C8-4c6b-9372-0760D3A8C08C")),  // Identify 
                            new RibbonItem(new Guid("ED5B0B59-2F5D-4b1a-BAD2-3CABEF073A6A"))   // Search
                        } 
                        ) { OnLauncherClick=OnInfoLauncherClick },
                    new RibbonGroupBox("Select", 
                        new RibbonItem[]{
                            new RibbonItem(new Guid("646860CF-4F82-424b-BF7D-822BE7A214FF"),"Middle"),  // Select
                            new RibbonItem(new Guid("F3DF8F45-4BAC-49ee-82E6-E10711029648"),"Middle"),  // Zoom 2 Selection
                            new RibbonItem(new Guid("16C05C00-7F21-4216-95A6-0B4B020D3B7D"),"Middle")   // Clear Selection
                        }
                        ),
                    new RibbonGroupBox("Tools", 
                        new RibbonItem[]{
                            new RibbonItem(new Guid("D185D794-4BC8-4f3c-A5EA-494155692EAC"),"Middle"),  // Measure
                            new RibbonItem(new Guid("2AC4447E-ACF3-453D-BB2E-72ECF0C8506E"),"Middle")   // XY
                        }
                        )
                }
                );
        }

        #region ICartoRibbonTab Member

        public string Header
        {
            get { return "Standard"; }
        }

        public List<RibbonGroupBox> Groups
        {
            get { return _groups; }
        }

        public int SortOrder
        {
            get { return 0; }
        }

        public bool IsVisible(IMapDocument mapDocument)
        {
            return mapDocument != null && mapDocument.FocusMap != null;
        }

        #endregion

        private void OnInfoLauncherClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (e is RibbonGroupBox.LauncherClickEventArgs && ((RibbonGroupBox.LauncherClickEventArgs)e).Hook is IMapDocument)
            {
                IMapDocument mapDocument = (IMapDocument)((RibbonGroupBox.LauncherClickEventArgs)e).Hook;

                if (!AppUIGlobals.IsAppReadOnly(mapDocument.Application))
                {
                    MapOptionPageIdentify dlg = new MapOptionPageIdentify(mapDocument);
                    dlg.ShowDialog();
                }
            }
        }
    }
}
