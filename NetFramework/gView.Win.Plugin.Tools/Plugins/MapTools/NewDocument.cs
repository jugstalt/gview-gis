using gView.Framework.Carto;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("D1A87DBA-00DB-4704-B67B-4846E6F03959")]
    public class NewDocument : gView.Framework.UI.ITool
    {
        IMapDocument _doc;

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.New", "New"); }
        }

        public bool Enabled
        {
            get
            {
                if (_doc == null || _doc.Application == null)
                {
                    return false;
                }

                if (_doc.Application is IMapApplication &&
                    ((IMapApplication)_doc.Application).ReadOnly == true)
                {
                    return false;
                }

                //LicenseTypes lt = _doc.Application.ComponentLicenseType("gview.desktop;gview.map");
                //return (lt == LicenseTypes.Licensed || lt == LicenseTypes.Express);
                return true;
            }
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
            get { return (new Buttons()).imageList1.Images[11]; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (_doc == null)
            {
                return Task.FromResult(true);
            }

            foreach (IMap map in _doc.Maps)
            {
                _doc.RemoveMap(map);
            }
            IMap newmap = new Map();
            _doc.AddMap(newmap);
            _doc.FocusMap = newmap;

            if (_doc.Application is IMapApplication)
            {
                ((IMapApplication)_doc.Application).DocumentFilename = String.Empty;
            }

            return Task.FromResult(true);
        }

        #endregion
    }
}
