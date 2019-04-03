using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Geometry;
using gView.Framework.UI.Dialogs;
using gView.Framework.UI;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormXY : Form
    {
        private static List<SpatialReferenceItem> _items = new List<SpatialReferenceItem>();
        private IMapDocument _doc;

        public FormXY(IMapDocument doc)
        {
            InitializeComponent();

            if (_items.Count == 0)
                _items.Add(new SpatialReferenceItem(SpatialReference.FromID("epsg:4326")));

            _doc = doc;
        }

        #region Members
        public IPoint GetPoint(ISpatialReference sRef)
        {
            return GeometricTransformer.Transform2D(
                coordControl1.Point,
                coordControl1.SpatialReference,
                sRef) as IPoint;
        }
        #endregion

        #region Events
        private void FormXY_Load(object sender, EventArgs e)
        {
            if (_doc == null ||
                _doc.FocusMap == null ||
                _doc.FocusMap.Display == null ||
                _doc.FocusMap.Display.Envelope == null)
                this.Close();

            foreach (SpatialReferenceItem item in _items)
            {
                if (item == null) return;

                cmbSRef.Items.Add(item);
                if (item.SpatialReference != null &&
                    _doc.FocusMap.Display.SpatialReference != null &&
                    item.SpatialReference.Equals(_doc.FocusMap.Display.SpatialReference))
                {
                    cmbSRef.SelectedIndex = cmbSRef.Items.Count - 1;
                }
            }

            if (cmbSRef.SelectedIndex == -1)
            {
                SpatialReferenceItem item = new SpatialReferenceItem(_doc.FocusMap.Display.SpatialReference);
                _items.Add(item);
                cmbSRef.Items.Add(item);
                cmbSRef.SelectedItem = item;
            }

            coordControl1.Init(_doc.FocusMap.Display.Envelope.Center,
                              ((SpatialReferenceItem)cmbSRef.SelectedItem).SpatialReference);
        }

        private void btnGetSRef_Click(object sender, EventArgs e)
        {
            ISpatialReference sRef =
                (cmbSRef.SelectedItem is SpatialReferenceItem) ?
                ((SpatialReferenceItem)cmbSRef.SelectedItem).SpatialReference :
                null;

            FormSpatialReference dlg = new FormSpatialReference(sRef.Clone() as ISpatialReference);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.SpatialReference != null &&
                    (sRef==null ||
                     !dlg.SpatialReference.Equals(sRef)))
                {
                    SpatialReferenceItem item = new SpatialReferenceItem(dlg.SpatialReference);
                    _items.Add(item);
                    cmbSRef.Items.Add(item);
                    cmbSRef.SelectedItem = item;
                }
            }
        }

        private void cmbSRef_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSRef.SelectedItem is SpatialReferenceItem)
            {
                coordControl1.SpatialReference =
                    ((SpatialReferenceItem)cmbSRef.SelectedItem).SpatialReference;
            }
        }
        #endregion

        #region ItemClasses
        private class SpatialReferenceItem
        {
            private ISpatialReference _sRef;

            public SpatialReferenceItem(ISpatialReference sRef)
            {
                _sRef = sRef;
            }

            public ISpatialReference SpatialReference
            {
                get { return _sRef; }
            }

            public override string ToString()
            {
                if (_sRef != null)
                    return (String.IsNullOrEmpty(_sRef.Description) ? _sRef.Name : _sRef.Description);

                return "Unknown";
            }
        }
        #endregion
    }
}