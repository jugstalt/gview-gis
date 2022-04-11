using gView.Framework.system;
using gView.Framework.UI;
using gView.Plugins.MapTools.Dialogs;
using System;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("F1B1602A-DD53-40a2-A504-61DC47A7B261")]
    public class PerformanceMonitor : ITool, IToolWindow
    {
        private IMapDocument _doc;
        private FormPerformanceMonitor _dlg = null;

        #region ITool Member

        public string Name
        {
            get { return "Performance Monitor"; }
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
            get { return ToolType.click; }
        }

        public object Image
        {
            get { return gView.Win.Plugin.Tools.Properties.Resources.time; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                if (_dlg == null)
                {
                    _dlg = new FormPerformanceMonitor(_doc);
                }
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolWindow Member

        public IDockableWindow ToolWindow
        {
            get { return _dlg; }
        }

        #endregion
    }
}
