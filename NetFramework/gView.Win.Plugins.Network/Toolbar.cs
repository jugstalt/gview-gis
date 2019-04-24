using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system;

namespace gView.Plugins.Network
{
    [RegisterPlugIn("818C6ECE-85CB-4ce8-963E-ED1799715E12")]
    public class Toolbar : IToolbar
    {
        private List<Guid> _guids;

        public Toolbar()
        {
            _guids = new List<Guid>();
            _guids.Add(new Guid("002124CB-804E-449e-BA7B-A3F3CBBBD154"));
            _guids.Add(new Guid("44762AEE-4F9C-4039-9577-372DC106B1C8"));
            _guids.Add(new Guid("457B8BC3-1F92-4512-BD09-9E6A870ADA93"));
            _guids.Add(new Guid("84A6A670-4044-43a0-94CE-05A244931D5C"));

            _guids.Add(new Guid("D7640425-DEF2-4c57-A165-464CDAB7C56E"));

            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            //_guids.Add(new Guid("D1E4E625-8AE6-4024-B955-DF4D8B13F791"));
            //_guids.Add(new Guid("D23D3243-91E0-497b-BF6B-DA9E49D9558C"));
            //_guids.Add(new Guid("1A5A89DD-1943-4e0a-BD8A-321533B6948F"));
            _guids.Add(new Guid("9A975F46-D727-495b-B752-9E079289E296"));
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("83A38411-27C7-4241-9F28-AC3005BFAFA8"));
            _guids.Add(new Guid("17475DC9-5A9B-4c90-8DE1-60654389F108"));
            _guids.Add(new Guid("158C5F28-B987-4d16-8C9D-A1FC6E70EB56"));
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("216D616B-FA15-4052-BFDE-16B6346C4B7F"));
        }
        #region IToolbar Member

        public string Name
        {
            get { return "Network"; }
        }

        public List<Guid> GUIDs
        {
            get
            {
                return _guids;
            }
            set
            {
                _guids = value;
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        #endregion
    }
}
