using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Globalisation;
using gView.Framework.system;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Framework.UI.Tools
{
    [gView.Framework.system.RegisterPlugIn("468EBDBB-CB41-447b-8D2A-7B32373928CD")]
    public class AddMap : ITool
    {
        IMapDocument _doc;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.NewMap", "New Map"); }
        }

        public bool Enabled
        {
            get
            {
                if (_doc == null || _doc.Application == null) return false;

                if (_doc.Application is IMapApplication &&
                    ((IMapApplication)_doc.Application).ReadOnly == true) return false;

                //LicenseTypes lt = _doc.Application.ComponentLicenseType("gview.desktop;gview.map");
                //return (lt == LicenseTypes.Licensed || lt == LicenseTypes.Express);
                return true;
            }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Win.Dialogs.Properties.Resources.layers; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_doc == null)
                return Task.FromResult(true);

            IMap map = new Map();
            map.Name = "Map" + (_doc.Maps.Count() + 1).ToString();

            _doc.AddMap(map);

            return Task.FromResult(true);
        }

        #endregion
    }
}
