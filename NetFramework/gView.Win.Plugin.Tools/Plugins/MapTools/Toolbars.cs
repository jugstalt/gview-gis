using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.IO;
using gView.Framework.Globalisation;
using gView.Framework.UI;

namespace gView.Plugins.MapTools
{
    [gView.Framework.system.RegisterPlugIn("1F1B9CEC-F957-43fd-B482-C41606160560")]
    public class MainToolbar : IToolbar, IPersistable
    {
        private bool _visible = true;
        private List<Guid> _guids;

        public MainToolbar()
        {
            _guids = new List<Guid>();
            _guids.Add(new Guid("D1A87DBA-00DB-4704-B67B-4846E6F03959"));  // New
            _guids.Add(new Guid("CEACE261-ECE4-4622-A892-58A5B32E5295"));  // Load
            _guids.Add(new Guid("FCA2C303-A0B6-4f36-BD21-E1C119EB9C8E"));  // Save
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("11BA5E40-A537-4651-B475-5C7C2D65F36E"));  // Add Dataset
            _guids.Add(new Guid("0728E12C-AC12-4264-9B47-ECE6BB0CFFA9"));  // TOC
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("97F70651-C96C-4b6e-A6AD-E9D3A22BFD45"));   // Copy
            _guids.Add(new Guid("3E36C2AD-2C58-42ca-A662-EE0C2DC1369D"));   // Cut
            _guids.Add(new Guid("0F8673B3-F1C9-4f5f-86C4-2B41FCBE535B"));   // Paste
            _guids.Add(new Guid("4F54D455-1C22-469e-9DBB-78DBBEF6078D"));   // Delete
            //_guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
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
            get { return LocalizedResources.GetResString("Toolbars.Standard", "Standard"); }
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

        #region IPersistable Members

        public string PersistID
        {
            get { return ""; }
        }

        public void Load(gView.Framework.IO.IPersistStream stream)
        {

        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {

        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("052F5F8C-94FC-4827-9147-F65FCC61FFB3")]
    public class ToolsToolbar : IToolbar, IPersistable
    {
        private bool _visible = true;
        private List<Guid> _guids;

        public ToolsToolbar()
        {
            _guids = new List<Guid>();
            _guids.Add(new Guid("09007AFA-B255-4864-AC4F-965DF330BFC4"));  // Zoomin Dyn
            _guids.Add(new Guid("51D04E6F-A13E-40b6-BF28-1B8E7C24493D"));  // Zoomout Dyn
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("3E2E9F8C-24FB-48f6-B80E-1B0A54E8C309"));  // SmartNavigation
            _guids.Add(new Guid("2680F0FD-31EE-48c1-B0F7-6674BAD0A688"));  // Pan
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("6351BCBE-809A-43cb-81AA-6414ED3FA459"));  // Zoomin Static
            _guids.Add(new Guid("E1C01E9D-8ADC-477b-BCD1-6B7BBA756D44"));  // Zoomout Static
            _guids.Add(new Guid("58AE3C1D-40CD-4f61-8C5C-0A955C010CF4"));  // Zoom To Full Extent
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("82F8E9C3-7B75-4633-AB7C-8F9637C2073D"));  // Back
            _guids.Add(new Guid("CFE66CDF-CD95-463c-8CD1-2541574D719A"));  // Forward
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("1E21835C-FD41-4e68-8462-9FAA66EA5A54"));  // QueryTuemeText
            _guids.Add(new Guid("51A2CF81-E343-4c58-9A42-9207C8DFBC01"));  // QueryThemeCombo
            _guids.Add(new Guid("F13D5923-70C8-4c6b-9372-0760D3A8C08C"));  // Identify 
            _guids.Add(new Guid("ED5B0B59-2F5D-4b1a-BAD2-3CABEF073A6A"));  // Search
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("D185D794-4BC8-4f3c-A5EA-494155692EAC"));  // Measure
            _guids.Add(new Guid("2AC4447E-ACF3-453D-BB2E-72ECF0C8506E"));  // XY
            _guids.Add(new Guid("00000000-0000-0000-0000-000000000000"));
            _guids.Add(new Guid("646860CF-4F82-424b-BF7D-822BE7A214FF"));  // Select
            _guids.Add(new Guid("F3DF8F45-4BAC-49ee-82E6-E10711029648"));  // Zoom 2 Selection
            _guids.Add(new Guid("16C05C00-7F21-4216-95A6-0B4B020D3B7D"));  // Clear Selection
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
            get { return LocalizedResources.GetResString("Toolbars.Tools", "Tools");  }
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

        void IPersistable.Load(IPersistStream stream)
        {
            //_visible = (bool)stream.Load("visible", true);
        }

        void IPersistable.Save(IPersistStream stream)
        {
            //stream.Save("visible", _visible);
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("9FBD6EB9-9D41-475f-BEC4-1D983B1590BC")]
    public class ScaleToolbar : IToolbar,IPersistable
    {
        private bool _visible = true;
        private List<Guid> _guids;

        public ScaleToolbar()
        {
            _guids = new List<Guid>();
            _guids.Add(new Guid("9AADD17B-CDD0-4111-BBC5-E31E060CE210"));  // ScaleText
            _guids.Add(new Guid("03058244-16EE-44dd-B185-5522281498F5"));  // Scale
        }
        #region IToolbar Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Toolbars.Scale", "Scale"); }
        }

        public List<Guid> GUIDs
        {
            get
            {
                return _guids;
            }
            set
            {
                
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            
        }

        public void Save(IPersistStream stream)
        {
            
        }

        #endregion
    }
}
