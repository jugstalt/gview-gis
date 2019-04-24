using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Network;

namespace gView.Plugins.Network
{
    [gView.Framework.system.RegisterPlugIn("5b2818b1-2a80-4390-9ca7-2e363cdc1027")]
    public class NetworkRibbonTab : ICartoRibbonTab
    {
        private List<RibbonGroupBox> _groups;

        public NetworkRibbonTab()
        {
            _groups = new List<RibbonGroupBox>(
                new RibbonGroupBox[]{
                    new RibbonGroupBox(String.Empty,
                        new RibbonItem[] {
                            new RibbonItem(new Guid("002124CB-804E-449e-BA7B-A3F3CBBBD154")),  
                            new RibbonItem(new Guid("44762AEE-4F9C-4039-9577-372DC106B1C8")),  
                            new RibbonItem(new Guid("457B8BC3-1F92-4512-BD09-9E6A870ADA93")), 
                            new RibbonItem(new Guid("84A6A670-4044-43a0-94CE-05A244931D5C"))
                        }
                        ),
                    new RibbonGroupBox(String.Empty,
                        new RibbonItem[]{
                            new RibbonItem(new Guid("D7640425-DEF2-4c57-A165-464CDAB7C56E")) 
                        }
                        ),
                    new RibbonGroupBox(String.Empty, 
                        new RibbonItem[]{
                            new RibbonItem(new Guid("9A975F46-D727-495b-B752-9E079289E296")),
                            new RibbonItem(new Guid("83A38411-27C7-4241-9F28-AC3005BFAFA8")),
                            new RibbonItem(new Guid("17475DC9-5A9B-4c90-8DE1-60654389F108")),
                            new RibbonItem(new Guid("158C5F28-B987-4d16-8C9D-A1FC6E70EB56")),
                            new RibbonItem(new Guid("216D616B-FA15-4052-BFDE-16B6346C4B7F"))  
                        }
                        )
                }
                );
        }

        #region ICartoRibbonTab Member

        public string Header
        {
            get { return "Network"; }
        }

        public List<RibbonGroupBox> Groups
        {
            get { return _groups; }
        }

        public int SortOrder
        {
            get { return 310; }
        }

        public bool IsVisible(IMapDocument mapDocument)
        {
            if (mapDocument == null || mapDocument.FocusMap == null)
                return false;

            foreach (IDatasetElement dsElement in mapDocument.FocusMap.MapElements)
            {
                if (dsElement.Class is INetworkFeatureClass)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
