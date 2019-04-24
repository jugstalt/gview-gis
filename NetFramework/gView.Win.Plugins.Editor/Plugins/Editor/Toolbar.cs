using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Globalisation;

namespace gView.Plugins.Editor
{
    [gView.Framework.system.RegisterPlugIn("414BD661-4128-42fe-BDA5-831E993C393B")]
    class Toolbar : IToolbar
    {
        private bool _visible = true;
        private List<Guid> _guids;

        public Toolbar()
        {
            _guids = new List<Guid>();

            //_guids.Add(new Guid("FE2AE24C-73F8-4da3-BBC7-45C2FCD3FE75"));  // Editmenu
            _guids.Add(new Guid("784148EB-04EA-413d-B11A-1A0F9A7EA4A0"));
            _guids.Add(new Guid("3C8A7ABC-B535-43d8-8F2D-B220B298CB17"));  // Target
            _guids.Add(new Guid("19396559-C13C-486c-B5F7-73DD5B12D5A8"));
            _guids.Add(new Guid("9B7D5E0E-88A5-40e2-977B-8A2E21875221"));  // Task
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("91392106-2C28-429c-8100-1E4E927D521C"));  // Modify Sketch
            _guids.Add(new Guid("FD340DE3-0BC1-4b3e-99D2-E8DCD55A46F2"));
            _guids.Add(new Guid("B576D3F9-F7C9-46d5-8A8C-16B3974F1BD7"));  // Attributes
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
           
            _guids.Add(new Guid("96099E8C-163E-46ec-BA33-41696BFAE4D5"));   // Save Feature
            _guids.Add(new Guid("AC4620D4-3DE4-49ea-A902-0B267BA46BBF"));   // Delete Feature
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));

            _guids.Add(new Guid("11DEE52F-F241-406e-BB40-9F247532E43D"));   // Delete Selected Feature
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));

            _guids.Add(new Guid("3B64107F-00C8-4f4a-B781-163FE9DA2D4B"));  // Create New
            //_guids.Add(new Guid("4F4A6AA1-89A6-498c-819A-0E52EF9AEA61"));
        }

        #region IToolbar Members

        //public bool Visible
        //{
        //    get
        //    {
        //        return _visible;
        //    }
        //    set
        //    {
        //        _visible = value;
        //    }
        //}

        public string Name
        {
            get { return LocalizedResources.GetResString("String.Editor", "Editor"); }
        }

        public List<Guid> GUIDs
        {
            get
            {
                return _guids;
            }
            set
            {
                _guids=value;
            }
        }

        #endregion

        #region IPersistable Members

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        #endregion
    }
}
