using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Collections.Generic;

namespace gView.Framework.Sys.UI
{
    [Serializable]
    public class ExplorerObjectSerialization : IExplorerObjectSerialization
    {
        private Guid _guid;
        private string _fullname;
        private List<Type> _exObjectTypes = new List<Type>();
        private List<Type> _ObjectTypes = new List<Type>();

        public ExplorerObjectSerialization()
        {
        }
        public ExplorerObjectSerialization(IExplorerObject exObject)
        {
            if (!PlugInManager.IsPlugin(exObject))
            {
                return;
            }

            _guid = PlugInManager.PlugInID(exObject);
            _fullname = exObject.FullName;
        }

        #region IExplorerObjectSerialization Member

        public Guid Guid
        {
            get { return _guid; }
        }

        public string FullName
        {
            get { return _fullname; }
        }

        public List<Type> ExplorerObjectTypes
        {
            get { return _exObjectTypes; }
        }

        public List<Type> ObjectTypes
        {
            get { return _ObjectTypes; }
        }

        #endregion
    }
}
