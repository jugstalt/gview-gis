using System;
using gView.Framework.UI;
using gView.Framework.system.UI;

namespace gView.Plugins.Tools.MapTools
{
    [gView.Framework.system.RegisterPlugIn("1a5381bb-95ed-4f92-90b7-c9ec947c9fec")]
    public class ToolPlugins : ITool, IToolControl
    {
        #region ITool Member

        public string Name
        {
            get { return "Plugins"; }
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
            
        }

        public void OnEvent(object MapEvent)
        {
            
        }

        #endregion

        #region IToolControl Member

        private PluginManagerControl _control;
        public object Control
        {
            get
            {
                if (_control == null)
                {
                    _control = new PluginManagerControl();
                }
                return _control;
            }
        }

        #endregion
    }
}
