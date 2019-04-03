using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Plugins.MapTools.Graphics;

namespace gView.Plugins.MapTools
{
    //[gView.Framework.system.RegisterPlugIn("C24147EC-F9F2-4a02-8C6D-A987B5ADB73E")]
    public class ThreeDToolbar : IToolbar
    {
        private bool _visible = true;
        private List<Guid> _guids;

        public ThreeDToolbar()
        {
            _guids = new List<Guid>();
            _guids.Add(new Guid("D533B387-E6FD-487e-8BF6-632099707123"));
        }

        #region IToolbar Member

        public string Name
        {
            get { return "3d"; }
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

    //[gView.Framework.system.RegisterPlugIn("D533B387-E6FD-487e-8BF6-632099707123")]
    public class Profile : ITool
    {

        #region ITool Member

        public string Name
        {
            get { return "Profile"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.click; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.polyline1; }
        }

        public void OnCreate(object hook)
        {
            
        }

        public void OnEvent(object MapEvent)
        {
            
        }

        #endregion
    }
}
