using gView.Framework.Data;
using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using System;
using System.Threading.Tasks;

namespace gView.DataSources.OGR
{
    internal class FeatureCursorV3 : IFeatureCursor
    {
        OSGeo_v3.OGR.DataSource _dataSource;
        OSGeo_v3.OGR.Layer _layer;

        public FeatureCursorV3(Dataset dataset, string layerName, IQueryFilter filter)
        {
            _dataSource = OSGeo_v3.OGR.Ogr.Open(dataset.ConnectionString, 0);
            _layer = _dataSource.GetLayerByName(layerName);

            if (_layer == null)
            {
                return;
            }

            if (filter is ISpatialFilter)
            {
                IEnvelope env = ((ISpatialFilter)filter).Geometry.Envelope;
                _layer.SetSpatialFilterRect(env.minx, env.miny, env.maxx, env.maxy);
                if (!String.IsNullOrEmpty(filter.WhereClause))
                {
                    _layer.SetAttributeFilter(filter.WhereClause);
                }
            }
            else
            {
                string where = filter.WhereClause;
                if (String.IsNullOrEmpty(where))
                {
                    where = null;
                }

                _layer.SetAttributeFilter(where);
            }
        }

        #region IFeatureCursor Member

        public Task<IFeature> NextFeature()
        {
            if (_layer == null)
            {
                return Task.FromResult<IFeature>(null);
            }

            OSGeo_v3.OGR.Feature ogrfeature = _layer.GetNextFeature();
            if (ogrfeature == null)
            {
                return Task.FromResult<IFeature>(null);
            }

            Feature feature = new Feature();
            feature.OID = (int)ogrfeature.GetFID();

            OSGeo_v3.OGR.FeatureDefn defn = ogrfeature.GetDefnRef();
            int fieldCount = defn.GetFieldCount();
            for (int i = 0; i < fieldCount; i++)
            {
                OSGeo_v3.OGR.FieldDefn fdefn = defn.GetFieldDefn(i);
                FieldValue fv = new FieldValue(fdefn.GetName());

                string fieldType = fdefn.GetFieldTypeName(fdefn.GetFieldType()).ToLower();
                switch (fieldType)
                {
                    case "integer":
                        fv.Value = ogrfeature.GetFieldAsInteger(i);
                        break;
                    case "real":
                        fv.Value = ogrfeature.GetFieldAsDouble(i);
                        break;
                    default:
                        fv.Value = ogrfeature.GetFieldAsString(i);
                        break;
                }
                feature.Fields.Add(fv);
            }

            if (feature.Shape == null)
            {
                OSGeo_v3.OGR.Geometry geom = ogrfeature.GetGeometryRef();
                if (geom != null)
                {
                    byte[] buffer = new byte[geom.WkbSize()];
                    geom.ExportToWkb(buffer);

                    feature.Shape = gView.Framework.OGC.OGC.WKBToGeometry(buffer);
                }
            }

            return Task.FromResult<IFeature>(feature);
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            try
            {
                if (_layer != null)
                {
                    _layer.ResetReading();
                    _layer.Dispose();
                    _layer = null;
                }
                if (_dataSource != null)
                {
                    _dataSource.Dispose();
                    _dataSource = null;
                }
            }
            catch { }
        }

        #endregion
    }
}
