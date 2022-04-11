using gView.Framework.Data;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Dialogs;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("306C83D1-E4FE-4474-A78E-F581D4304937")]
    public class FeatureClassDataTable : gView.Framework.UI.IDatasetElementContextMenuItem
    {
        IMapDocument _doc;

        #region ILayerContextMenuItem Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.DataTable", "Data Table...");
            }
        }

        public bool Enable(object element)
        {
            if ((element is ITableLayer) || (element is IFeatureLayer))
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

        public Task<bool> OnEvent(object layer, object dataset)
        {
            if (dataset == null || !(layer is ILayer) || _doc == null)
            {
                return Task.FromResult(true);
            }

            ITableClass table = null;
            if (layer is IFeatureLayer)
            {
                table = ((IFeatureLayer)layer).FeatureClass;
            }
            else if (layer is ITableLayer)
            {
                table = ((ITableLayer)layer).TableClass;
            }

            if (table == null)
            {
                return Task.FromResult(true);
            }

            string Title = ((ILayer)layer).Title;



            if (_doc.Application is IMapApplication)
            {
                IMapApplication appl = (IMapApplication)_doc.Application;

                foreach (IDockableWindow win in appl.DockableWindows)
                {
                    if (win is FormDataTable)
                    {
                        if (((FormDataTable)win).TableClass == table)
                        {
                            // Show The Window
                            appl.ShowDockableWindow(win);
                            return Task.FromResult(true);
                        }
                    }
                }
            }

            FormDataTable dlg = new FormDataTable((ILayer)layer);
            dlg.Text = dlg.Name = Title;
            dlg.MapDocument = _doc;

            /*
			if(_doc!=null) 
			{
				if(_doc.DocumentWindow!=null) 
				{
					_doc.DocumentWindow.AddChildWindow(dlg);	
				}
			}
			dlg.Show();
            */

            if (_doc.Application is IMapApplication)
            {
                IMapApplication appl = (IMapApplication)_doc.Application;

                appl.AddDockableWindow(dlg, PlugInManager.PlugInID(new DataTableContainer()).ToString());
                appl.ShowDockableWindow(dlg);
            }

            _doc = null;

            return Task.FromResult(true);
        }

        public object Image
        {
            get { return gView.Win.Plugin.Tools.Properties.Resources.table; }
        }

        public int SortOrder
        {
            get { return 22; }
        }
        #endregion
    }
}
