using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.DataSources.OGR
{
    internal class FeatureCursor : IFeatureCursor
    {
        OSGeo.OGR.Layer _layer;
        public FeatureCursor(OSGeo.OGR.Layer layer, IQueryFilter filter)
        {
            if (layer == null) return;

            _layer = layer;
            _layer.ResetReading();

            if (filter is ISpatialFilter)
            {
                IEnvelope env=((ISpatialFilter)filter).Geometry.Envelope;
                _layer.SetSpatialFilterRect(env.minx, env.miny, env.maxx, env.maxy);
                if (String.IsNullOrEmpty(filter.WhereClause))
                    _layer.SetAttributeFilter(null);
                else
                    _layer.SetAttributeFilter(filter.WhereClause);
            }
            else
            {
                string where = filter.WhereClause;
                if (String.IsNullOrEmpty(where)) where = null;
                _layer.SetAttributeFilter(where);
                _layer.SetSpatialFilter(null);
            }
        }

        #region IFeatureCursor Member

        public IFeature NextFeature
        {
            get
            {
                if (_layer == null) return null;
                OSGeo.OGR.Feature ogrfeature = _layer.GetNextFeature();
                if (ogrfeature == null) return null;

                Feature feature = new Feature();
                feature.OID = ogrfeature.GetFID();

                OSGeo.OGR.FeatureDefn defn = ogrfeature.GetDefnRef();
                int fieldCount = defn.GetFieldCount();
                for (int i = 0; i < fieldCount; i++)
                {
                    OSGeo.OGR.FieldDefn fdefn = defn.GetFieldDefn(i);
                    FieldValue fv = new FieldValue(fdefn.GetName());

                    switch (fdefn.GetFieldTypeName(fdefn.GetFieldType()).ToLower())
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

                OSGeo.OGR.Geometry geom = ogrfeature.GetGeometryRef();
                feature.Shape = gView.Framework.OGC.GML.GeometryTranslator.GML2Geometry(geom.ExportToGML(), GmlVersion.v1);

                return feature;
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            try
            {
                if (_layer == null) return;
                _layer.ResetReading();
                _layer = null;
            }
            catch { }
        }

        #endregion
    }
}
