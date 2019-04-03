using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormDeleteDatasetElements : Form
    {
        public FormDeleteDatasetElements(List<IDatasetElement> datasetElements)
        {
            InitializeComponent();

            if (datasetElements == null) return;
            foreach (IDatasetElement datasetElement in datasetElements)
            {
                if (datasetElement == null) continue;

                int imgIndex = 0;
                if (datasetElement.Class is IFeatureClass)
                {
                    switch (((IFeatureClass)datasetElement.Class).GeometryType)
                    {
                        case gView.Framework.Geometry.geometryType.Point:
                            imgIndex = 1;
                            break;
                        case gView.Framework.Geometry.geometryType.Polyline:
                            imgIndex = 2;
                            break;
                        case gView.Framework.Geometry.geometryType.Polygon:
                            imgIndex = 3;
                            break;
                    }
                }
                else if (datasetElement.Class != null)
                {
                    imgIndex = 4;
                }

                lstObjects.Items.Add(new DatasetItemListViewItem(datasetElement, imgIndex));
            }
        }

        public List<IDatasetElement> Selected
        {
            get
            {
                List<IDatasetElement> list = new List<IDatasetElement>();
                foreach (DatasetItemListViewItem item in lstObjects.Items)
                {
                    if (item.Checked)
                        list.Add(item.DatasetElement);
                }
                return list;
            }
        }

        #region ItemClass
        private class DatasetItemListViewItem : ListViewItem
        {
            IDatasetElement _datasetElement = null;

            public DatasetItemListViewItem(IDatasetElement datasetElement, int imageIndex)
            {
                if (datasetElement == null) return;
                base.Text = datasetElement.Title;
                base.ImageIndex = imageIndex;
                base.Checked = true;

                _datasetElement = datasetElement;
            }

            public IDatasetElement DatasetElement
            {
                get { return _datasetElement; }
            }
        }
        #endregion
    }

    
}