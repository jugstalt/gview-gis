using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Interoperability.OGC.Dataset.WMS;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    public class LayerContextMenu : IDatasetElementContextMenuItem
    {
        private IMapDocument _doc = null;

        #region IDatasetElementContextMenuItem Member

        public string Name
        {
            get { return "WMS Coordinate System"; }
        }

        public bool Enable(object element)
        {
            if(element is IDatasetElement && ((IDatasetElement)element).Class is WMSClass)
                return true;

            return false;
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

        public void OnEvent(object element,object dataset)
        {
            if (!this.Enable(element))
                return;

            FormWMSCoordinateSystem dlg = new FormWMSCoordinateSystem(((IDatasetElement)element).Class as WMSClass);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ((WMSClass)((IDatasetElement)element).Class).SRSCode = dlg.SRSCode;

                if (_doc != null)
                {
                    if (_doc.FocusMap.Display.SpatialReference == null)
                    {
                        _doc.FocusMap.Display.SpatialReference = ((WMSClass)((IDatasetElement)element).Class).SpatialReference;
                    }
                    else
                    {
                        if (MessageBox.Show(null, "Set this Coordinate System for the current map also?", "", MessageBoxButtons.YesNo)==DialogResult.Yes)
                        {
                            _doc.FocusMap.Display.SpatialReference = ((WMSClass)((IDatasetElement)element).Class).SpatialReference;
                        }
                    }

                    if(_doc.Application is IMapApplication)
                        ((IMapApplication)_doc.Application).RefreshActiveMap(gView.Framework.Carto.DrawPhase.All);
                }
            }
        }

        public object Image
        {
            get { return null; }
        }

        public int SortOrder
        {
            get { return 85; }
        }

        #endregion
    }
}
