using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Dialogs;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("0F9E298A-C82E-4cae-B1EE-142CF1295D9E")]
    public class FeatureLayerProperties : gView.Framework.UI.IDatasetElementContextMenuItem
    {
        IMapDocument _doc = null;

        #region ILayerContextMenuItem Member

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Properties", "Properties...");
            }
        }

        public bool Enable(object element)
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

            if (element is ILayer)
            {
                return true;
            }

            return false;
        }

        public bool Visible(object element)
        {
            return Enable(element);
        }

        public Task<bool> OnEvent(object layer, object dataset)
        {
            if (!(layer is ILayer) || (!(dataset is IDataset) && !(layer is IGroupLayer)))
            {
                return Task.FromResult(true);
            }

            if (layer is ILayer)
            {
                var map = _doc.MapFromDataset(dataset as IDataset) ?? _doc.MapFromLayer((ILayer)layer);

                FormLayerProperties dlg = new FormLayerProperties(_doc, map, (IDataset)dataset, (ILayer)layer);

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (_doc != null)
                    {
                        if (_doc.Application is IMapApplication)
                        {
                            ((IMapApplication)_doc.Application).RefreshTOCElement((ILayer)layer);
                            ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                        }
                        if (_doc.FocusMap != null)
                        {
                            _doc.FocusMap.TOC.Reset();
                        }
                    }
                }
            }

            return Task.FromResult(true);
        }

        public object Image
        {
            get { return gView.Win.Plugin.Tools.Properties.Resources.document_properties; }
        }

        public int SortOrder
        {
            get { return 99; }
        }

        #endregion
    }
}
