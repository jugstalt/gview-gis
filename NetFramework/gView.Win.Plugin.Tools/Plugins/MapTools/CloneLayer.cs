using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.Windows.Forms;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Plugins.MapTools.Dialogs;
using gView.Framework.Carto.UI;

namespace gView.Plugins.MapTools
{
    [gView.Framework.system.RegisterPlugIn("FECB7F1E-B5D1-4b96-A0FF-4FDCF5BFC18E")]
    class CloneLayer : IDatasetElementContextMenuItem
    {
        private IMapDocument _doc = null;

        #region IDatasetElementContextMenuItem Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.CloneLayer", "Clone"); }
        }

        public bool Enable(object element)
        {
            if (_doc!=null && _doc.Application is IMapApplication &&
                    ((IMapApplication)_doc.Application).ReadOnly == true) return false;

            //LicenseTypes lt = _doc.Application.ComponentLicenseType("gview.desktop;gview.map");
            //if (lt == LicenseTypes.Licensed || lt == LicenseTypes.Express)
            {
                return (_doc != null && _doc.FocusMap != null && _doc.FocusMap.TOC != null && element != null);
            }
            //return false; 
        }

        public bool Visible(object element)
        {
            if (_doc != null && _doc.Application is IMapApplication &&
                    ((IMapApplication)_doc.Application).ReadOnly == true) return false;

            //LicenseTypes lt = _doc.Application.ComponentLicenseType("gview.desktop;gview.map");
            //if (lt == LicenseTypes.Licensed || lt == LicenseTypes.Express)
            {
                return true;
            }

            //return false;
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
        }

        public void OnEvent(object element, object dataset)
        {
            if (!(element is IDatasetElement) || !(dataset is IDataset))
                return;

            TOC toc= _doc.FocusMap.TOC as TOC;
            if(toc==null) return;

            ITOCElement tocElement = toc.GetTOCElement(((IDatasetElement)element).Class);
            if (tocElement == null) return;

            ILayer newLayer = LayerFactory.Create(((IDatasetElement)element).Class);
            
            if (newLayer is IFeatureLayer && element is IFeatureLayer)
            {
                if (((IFeatureLayer)element).Joins != null)
                {
                    ((IFeatureLayer)newLayer).Joins = ((IFeatureLayer)element).Joins.Clone() as FeatureLayerJoins;
                }
                if (((IFeatureLayer)element).FilterQuery != null)
                {
                    QueryFilter filter = new QueryFilter();
                    filter.WhereClause = ((IFeatureLayer)element).FilterQuery.WhereClause;
                    ((IFeatureLayer)newLayer).FilterQuery = filter;
                }
            }
            if (newLayer == null) return;

            _doc.FocusMap.AddLayer(newLayer);

            if (_doc.Application is IMapApplication)
                ((IMapApplication)_doc.Application).RefreshActiveMap(Framework.Carto.DrawPhase.All);
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.clone; }
        }

        public int SortOrder
        {
            get { return 65; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("3EDAD926-80DA-44a6-8CA0-FDBB18C39C06")]
    class SplitLayerWithFilter : IDatasetElementContextMenuItem
    {
        private IMapDocument _doc = null;

        #region IDatasetElementContextMenuItem Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.SplitWithFilter", "Split With Filter..."); }
        }

        public bool Enable(object element)
        {
            if (_doc != null && _doc.Application is IMapApplication &&
                    ((IMapApplication)_doc.Application).ReadOnly == true) return false;

            return (element is IFeatureLayer && 
                _doc != null && _doc.FocusMap != null && _doc.FocusMap.TOC != null && element != null); 
        }

        public bool Visible(object element)
        {
            return Enable(element);
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
        }

        public void OnEvent(object element, object dataset)
        {
            if (!(element is IDatasetElement) || !(dataset is IDataset))
                return;

            TOC toc = _doc.FocusMap.TOC as TOC;
            if (toc == null) return;

            ITOCElement tocElement = toc.GetTOCElement(((IDatasetElement)element).Class);
            if (tocElement == null) return;
            ITOCElement parentTocElement = tocElement.ParentGroup;

            //IDatasetElement e = ((IDataset)dataset)[((IDatasetElement)element).Title];
            //if (e == null) return;
 
            if (!(element is ILayer)) return;
            FormSplitLayerWithFilter dlg = new FormSplitLayerWithFilter(_doc, element as ILayer);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (FormSplitLayerWithFilter.FilterExpessionItem expression in dlg.FilterExpressions)
                {
                    ILayer newLayer = LayerFactory.Create(((IDatasetElement)element).Class);
                    if (newLayer is IFeatureLayer &&
                        element is IFeatureLayer && ((IFeatureLayer)element).Joins != null)
                    {
                        ((IFeatureLayer)newLayer).Joins = ((IFeatureLayer)element).Joins.Clone() as FeatureLayerJoins;
                    }

                    if (newLayer is IFeatureLayer)
                    {
                        QueryFilter filter = new QueryFilter();
                        filter.WhereClause = expression.Filter;
                        ((IFeatureLayer)newLayer).FilterQuery = filter;

                        _doc.FocusMap.AddLayer(newLayer);
                        tocElement = toc.GetTOCElement(newLayer);
                        tocElement.Name = expression.Text;
                        toc.Add2Group(tocElement, parentTocElement);
                    }
                }

                if (_doc.Application is IMapApplication)
                    ((IMapApplication)_doc.Application).RefreshActiveMap(Framework.Carto.DrawPhase.All);
            }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.split; }
        }

        public int SortOrder
        {
            get { return 68; }
        }

        #endregion
    }
}
