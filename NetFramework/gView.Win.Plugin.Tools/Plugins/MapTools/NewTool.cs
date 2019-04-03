using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;

namespace gView.Plugins.MapTools
{
    [gView.Framework.system.RegisterPlugIn("d0da246e-89a1-40ba-9e6b-c4d0b97640a1")]
    public class NewTool : ITool, IToolControl
    {
        IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return "New"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public void OnEvent(object MapEvent)
        {
            
        }

        #endregion

        #region IToolControl Member

        public object Control
        {
            get
            {
                return new Controls.NewToolControl(_doc);
            }
        }

        #endregion
    }
}
