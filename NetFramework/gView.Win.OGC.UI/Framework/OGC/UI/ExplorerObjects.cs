using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using System.Xml;
using gView.Framework.Data;
using gView.Framework.OGC.DB;
using gView.Framework.UI;
using gView.Framework.system.UI;
using System.Threading.Tasks;

namespace gView.Framework.OGC.UI
{
    public interface IOgcGroupExplorerObject : IExplorerObject
    {
    }

    [gView.Framework.system.RegisterPlugIn("D6038DDE-DCB9-4cab-ADAC-C80EA323527D")]
    public class OGCExplorerGroupObject : ExplorerParentObject, IExplorerGroupObject
    {
        private IExplorerIcon _icon = new OGCExplorerGroupObjectIcon();

        public OGCExplorerGroupObject() : base(null, null, 0) { }

        #region IExplorerGroupObject Members

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        #endregion

        #region IExplorerObject Members

        public string Name
        {
            get { return "OGC"; }
        }

        public string FullName
        {
            get { return "OGC"; }
        }

        public string Type
        {
            get { return "OGC Connections"; }
        }

        public new object Object
        {
            get { return null; }
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerParentObject Members

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            PlugInManager compMan = new PlugInManager();
            foreach (var compType in compMan.GetPlugins(Plugins.Type.IExplorerObject))
            {
                IExplorerObject exObject = compMan.CreateInstance<IExplorerObject>(compType);
                if (!(exObject is IOgcGroupExplorerObject)) continue;

                base.AddChildObject(exObject);
            }

            return true;
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return Task.FromResult<IExplorerObject>(cache[FullName]);

            if (this.FullName == FullName)
            {
                OGCExplorerGroupObject exObject = new OGCExplorerGroupObject();
                cache.Append(exObject);
                return Task.FromResult<IExplorerObject>(exObject);
            }

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

        private class OGCExplorerGroupObjectIcon : IExplorerIcon
        {
            #region IExplorerIcon Members

            public Guid GUID
            {
                get { return new Guid("A64A549C-961F-4164-87C4-97D9FD1C24A2"); }
            }

            public System.Drawing.Image Image
            {
                get { return global::gView.Win.OGC.UI.Properties.Resources.earth; }
            }

            #endregion
        }
    }
}
