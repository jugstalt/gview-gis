using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.system;
using System.Windows.Forms;
using gView.Framework.Carto;
using gView.Framework.Globalisation;
using System.Threading.Tasks;

namespace gView.Plugins.DbTools.Migrate
{
    [RegisterPlugInAttribute("C15AA28A-07AE-4e5b-9F06-4C0254C44056")]
    class MirgrateMapToFeatureDatabase : IMapContextMenuItem
    {
        private IMapDocument _doc = null;

        #region IMapContextMenuItem Member

        public string Name
        {
            get
            {
                //return "Checkout/Clone Map To Feature Database...";
                return LocalizedResources.GetResString("Tools.MirgrateMapToFeatureDatabase", "Checkout/Clone Map To Feature Database...");
            }
        }

        public bool Enable(object element)
        {
            return element is IMap && ((IMap)element).MapElements!=null && ((IMap)element).MapElements.Count > 0;
        }

        public bool Visible(object element)
        {
            return Enable(element);
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public Task<bool> OnEvent(object map, object parent)
        {
            if (_doc == null || !(map is IMap))
                return Task.FromResult(false);

            MigrateMapToFeatureDatabaseDialog dlg = new MigrateMapToFeatureDatabaseDialog(_doc, (IMap)map);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }

            return Task.FromResult(true);
        }

        public object Image
        {
            get { return global::gView.Win.Plugins.DbTools.Properties.Resources.database_go; }
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 85; }
        }

        #endregion
    }
}
