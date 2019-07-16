using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Data;
using System.Threading.Tasks;

namespace gView.Plugins.DbTools.Joins
{
    [RegisterPlugInAttribute("18D3A030-FBE6-4dbd-AD23-0F6C291EFCC9")]
    class FeatureLayerJoins : IDatasetElementContextMenuItem
    {
        private IMapDocument _doc = null;

        #region IDatasetElementContextMenuItem Member

        public string Name
        {
            get { return "Joins"; }
        }

        public bool Enable(object element)
        {
            return element is IFeatureLayer;
        }

        public bool Visible(object element)
        {
            return element is IFeatureLayer;
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = hook as IMapDocument;
            }
        }

        public Task<bool> OnEvent(object element, object dataset)
        {
            if (_doc == null || !(element is IFeatureLayer) && !(((IFeatureLayer)element).Class is IFeatureClass))
                return Task.FromResult(false);

            FeatureLayerJoinsDialog dlg = new FeatureLayerJoinsDialog(_doc, (IFeatureLayer)element);
            dlg.ShowDialog();

            return Task.FromResult(true);
        }

        public object Image
        {
            get { return global::gView.Win.Plugins.DbTools.Properties.Resources.sql_join; }
        }

        public int SortOrder
        {
            get { return 45; }
        }

        #endregion
    }
}
