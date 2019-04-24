using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Globalisation;

namespace gView.Plugins.Snapping
{
    [gView.Framework.system.RegisterPlugIn("B903AF52-4CA7-4286-A596-775C4690C93D")]
    class Toolbar : IToolbar
    {
        private List<Guid> _guids;

        public Toolbar()
        {
            _guids = new List<Guid>();

            _guids.Add(new Guid("8C5AD6C8-8991-447c-9313-5E0FAC6EA2BB"));
            _guids.Add(new Guid("9CDE6BD1-317E-478b-8828-B169A6688CC5"));
            //_guids.Add(new Guid("35BCECD1-393F-443f-B15D-DC1DC6AD9564"));
            _guids.Add(new Guid("D678C377-79A0-49e5-88A3-6635FD7B522C")); 
        }

        #region IToolbar Member

        public string Name
        {
            get { return LocalizedResources.GetResString("String.Snapping", "Snapping"); }
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
