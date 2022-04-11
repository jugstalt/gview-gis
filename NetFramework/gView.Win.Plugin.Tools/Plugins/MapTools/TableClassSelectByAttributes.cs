using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Dialogs;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("44A1902B-CDC6-43d7-9D48-3DA80437445E")]
    public class TableClassSelectByAttributes : gView.Framework.UI.IDatasetElementContextMenuItem
    {
        IMapDocument _doc;

        #region IDatasetElementContextMenuItem Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.SelectByAttributes", "Select By Attributes..."); }
        }

        public bool Enable(object element)
        {
            if (((element is ITableLayer) || (element is IFeatureLayer)) && element is IFeatureSelection)
            {
                return true;
            }

            return false;
        }

        public bool Visible(object element)
        {
            return Enable(element);
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        async public Task<bool> OnEvent(object element, object dataset)
        {
            if (element is IFeatureSelection && element is IFeatureLayer)
            {
                FormQueryBuilder dlg = await FormQueryBuilder.CreateAsync((IFeatureLayer)element);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    QueryFilter filter = new QueryFilter();
                    filter.WhereClause = dlg.whereClause;

                    await ((IFeatureSelection)element).Select(filter, dlg.combinationMethod);
                    ((IFeatureSelection)element).FireSelectionChangedEvent();

                    if (_doc != null)
                    {
                        if (_doc.Application is IMapApplication)
                        {
                            await ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Selection);
                        }
                    }
                }
            }

            return true;
        }

        public object Image
        {
            get { return gView.Win.Plugin.Tools.Properties.Resources.sql; }
        }

        public int SortOrder
        {
            get { return 23; }
        }

        #endregion
    }
}
