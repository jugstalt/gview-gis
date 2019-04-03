using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;

namespace gView.Plugins.Tools.RibbonTabs
{
    [gView.Framework.system.RegisterPlugIn("3e9ebc6d-4f71-4ecc-aa4f-cbec2f5b507d")]
    public class GraphicsRibbonTab : ICartoRibbonTab
    {
        private List<RibbonGroupBox> _groups;

        public GraphicsRibbonTab()
        {
            _groups = new List<RibbonGroupBox>(
                new RibbonGroupBox[]{
                    new RibbonGroupBox("Draw",
                        new RibbonItem[] {
                            new RibbonItem(new Guid("895B7C49-6612-45ca-998A-1B897696799C")),  
                            new RibbonItem(new Guid("FEEEE362-116B-406b-8D94-D817A9BAC121")),  
                            new RibbonItem(new Guid("21D26293-3994-49ba-90F1-A27048F23011")), 
                            new RibbonItem(new Guid("4FFEB659-3AF4-4aa0-99FD-69A847E0F37A"))
                        }
                        ),
                    new RibbonGroupBox("Shape Styles",
                        new RibbonItem[]{
                            new RibbonItem(new Guid("681FEEC3-5714-45eb-9C86-39F5948783D8")),  
                            new RibbonItem(new Guid("03095F5A-527B-4dd6-93EF-11E7A75A7B86")),  
                            new RibbonItem(new Guid("77886897-7288-4488-AF76-4A874B555039")),  
                            new RibbonItem(new Guid("BEAABECF-3019-4b88-849E-9EA69E4EA0E5")) 
                        }
                        ),
                    new RibbonGroupBox("Text Styles", 
                        new RibbonItem[]{
                            new RibbonItem(new Guid("F3E5AF88-2BCE-44a5-AABF-E6ABF2EE1D42")),
                            new RibbonItem(new Guid("39758B9D-4163-442e-A43C-F8293BC2D630")),
                            new RibbonItem(new Guid("F87966B0-07E9-41a7-97E0-E97A659B8DA0"))  
                        }
                        ),
                    new RibbonGroupBox("Clipboard",
                        new RibbonItem[] {
                            new RibbonItem(new Guid("97F70651-C96C-4b6e-A6AD-E9D3A22BFD45"), "Middle"),  // Copy
                            new RibbonItem(new Guid("0F8673B3-F1C9-4f5f-86C4-2B41FCBE535B"), "Middle"),  // Paste
                            new RibbonItem(new Guid("3E36C2AD-2C58-42ca-A662-EE0C2DC1369D"), "Middle")  // Cut
                        }
                        )
                }
                );
        }

        #region ICartoRibbonTab Member

        public string Header
        {
            get { return "Graphics"; }
        }

        public List<RibbonGroupBox> Groups
        {
            get { return _groups; }
        }

        public int SortOrder
        {
            get { return 100; }
        }

        
        public bool IsVisible(IMapDocument mapDocument)
        {
            return mapDocument != null && mapDocument.FocusMap != null;
        }

        #endregion
    }
}
