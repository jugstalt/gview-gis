using gView.Framework.Common.Diagnostics;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Data;
using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public abstract class FeatureCursor : IFeatureCursorSkills, IDiagnostics
    {
        private IGeometricTransformer _transformer = null;
        private bool _knowsFunctions = true;

        public FeatureCursor(
                ISpatialReference fcSRef, 
                ISpatialReference toSRef,
                IDatumTransformations datumTransformations)
        {
            if (fcSRef != null && !fcSRef.Equals(toSRef))
            {
                _transformer = GeometricTransformerFactory.Create(datumTransformations);
                //_transformer.FromSpatialReference = fcSRef;
                //_transformer.ToSpatialReference = toSRef;
                _transformer.SetSpatialReferences(fcSRef, toSRef);
            }
        }

        protected void Transform(IFeature feature)
        {
            if (feature != null && _transformer != null && feature.Shape != null)
            {
                feature.Shape = _transformer.Transform2D(feature.Shape) as IGeometry;
            }
        }

        protected IFeature CloneAndTransform(IFeature feature)
        {
            if (feature == null) return null;

            Feature cloned = new Feature();

            foreach (var f in feature.Fields ?? [])
            {
                cloned.Fields.Add(new FieldValue(f.Name, f.Value));
            }

            cloned.Shape = (IGeometry)feature.Shape?.Clone();

            Transform(cloned);

            return cloned;
        }

        protected IFeature CloneIfTransform(IFeature feature)
        {
            if (feature != null && _transformer != null && feature.Shape != null)
            {
                Feature cloned = new Feature();

                foreach (var f in feature.Fields)
                {
                    cloned.Fields.Add(new FieldValue(f.Name, f.Value));
                }

                cloned.Shape = (IGeometry)feature.Shape.Clone();
                cloned.Shape = _transformer.Transform2D(cloned.Shape) as IGeometry;

                return cloned;
            }

            return feature;
        }

        protected ICancelTracker CancelTracker { get; set; }

        #region IFeatureCursor Member

        abstract public Task<IFeature> NextFeature();

        #endregion

        #region IDisposable Member

        virtual public void Dispose()
        {
            if (_transformer != null)
            {
                _transformer.Release();
            }
        }

        #endregion

        #region IFeatureCursorSkills Member

        public bool KnowsFunctions
        {
            get { return _knowsFunctions; }
            set { _knowsFunctions = false; }
        }

        #endregion

        #region DataTable Functions

        async public static Task<DataTable> ToDataTable(IFeatureCursor cursor)
        {
            DataTable tab = new DataTable();
            tab.Columns.Add("#SHAPE#", typeof(object));
            tab.Columns.Add("#OID#", typeof(int));

            if (cursor != null)
            {
                IFeature feature;
                while ((feature = await cursor.NextFeature()) != null)
                {
                    #region Columns
                    foreach (FieldValue fv in feature.Fields)
                    {
                        if (fv.Value == null || fv.Value == DBNull.Value)
                        {
                            continue;
                        }

                        if (tab.Columns[fv.Name] == null)
                        {
                            tab.Columns.Add(fv.Name, fv.Value.GetType());
                        }
                    }
                    #endregion

                    DataRow row = tab.NewRow();
                    foreach (FieldValue fv in feature.Fields)
                    {
                        row[fv.Name] = fv.Value;
                    }

                    row["#SHAPE#"] = feature.Shape;
                    row["#OID#"] = feature.OID;

                    tab.Rows.Add(row);
                }
            }

            return tab;
        }

        public class DataRowCursor : IFeatureCursor
        {
            private DataRow[] _rows;
            private int _pos = 0;

            public DataRowCursor(DataRow[] rows)
            {
                _rows = rows;
            }

            #region IFeatureCursor Member

            public Task<IFeature> NextFeature()
            {
                if (_rows == null || _pos >= _rows.Length)
                {
                    return null;
                }

                DataRow row = _rows[_pos++];
                DataTable tab = row.Table;
                Feature feature = new Feature();

                if (tab.Columns["#OID#"] != null)
                {
                    feature.OID = (int)row["#OID#"];
                }

                if (tab.Columns["#SHAPE#"] != null)
                {
                    feature.Shape = row["#SHAPE#"] as IGeometry;
                }

                foreach (DataColumn col in tab.Columns)
                {
                    if (col.ColumnName == "#OID#" || col.ColumnName == "#SHAPE#")
                    {
                        continue;
                    }

                    feature.Fields.Add(new FieldValue(col.ColumnName, row[col.ColumnName]));
                }

                return Task.FromResult<IFeature>(feature);
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {

            }

            #endregion
        }

        #endregion

        #region IDiagnostics

        public DiagnosticParameters DiagnosticParameters { get; protected set; }

        #endregion
    }
}
