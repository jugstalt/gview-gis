using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Threading.Tasks;

namespace gView.DataSources.OGR
{
    internal class FeatureCursor : IFeatureCursor
    {
        OSGeo_v1.OGR.Layer _layer;
        public FeatureCursor(OSGeo_v1.OGR.Layer layer, IQueryFilter filter)
        {
            if (layer == null)
            {
                return;
            }

            _layer = layer;
            _layer.ResetReading();

            if (filter is ISpatialFilter)
            {
                IEnvelope env=((ISpatialFilter)filter).Geometry.Envelope;
                _layer.SetSpatialFilterRect(env.minx, env.miny, env.maxx, env.maxy);
                if (String.IsNullOrEmpty(filter.WhereClause))
                {
                    _layer.SetAttributeFilter(null);
                }
                else
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
                _layer.SetSpatialFilter(null);
            }
        }

        #region IFeatureCursor Member

        public Task<IFeature> NextFeature()
        {
            if (_layer == null)
            {
                return Task.FromResult<IFeature>(null);
            }

            OSGeo_v1.OGR.Feature ogrfeature = _layer.GetNextFeature();
            if (ogrfeature == null)
            {
                return Task.FromResult<IFeature>(null);
            }

            Feature feature = new Feature();
            feature.OID = (int)ogrfeature.GetFID();

            OSGeo_v1.OGR.FeatureDefn defn = ogrfeature.GetDefnRef();
            int fieldCount = defn.GetFieldCount();
            for (int i = 0; i < fieldCount; i++)
            {
                OSGeo_v1.OGR.FieldDefn fdefn = defn.GetFieldDefn(i);
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
                        //if (fv.Name == "geom")
                        //{
                        //    var geom = ogrfeature.GetFieldAsString(i).ToByteArray();
                        //    if (geom != null)
                        //    {
                        //        feature.Shape = gView.Framework.OGC.OGC.WKBToGeometry(geom);
                        //    }
                        //}
                        //else
                        {
                            fv.Value = ogrfeature.GetFieldAsString(i);
                        }
                        break;
                }
                feature.Fields.Add(fv);
            }

            if (feature.Shape == null)
            {
                OSGeo_v1.OGR.Geometry geom = ogrfeature.GetGeometryRef();
                if (geom != null)
                {
                    feature.Shape = gView.Framework.OGC.GML.GeometryTranslator.GML2Geometry(geom.ExportToGML(), GmlVersion.v1);
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
                if (_layer == null)
                {
                    return;
                }

                _layer.ResetReading();
                _layer = null;
            }
            catch { }
        }

        #endregion
    }
}
