using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde
{
    public class SdeFeatureClass : gView.Framework.OGC.DB.OgcSpatialFeatureclass, IFeatureClassPerformanceInfo
    {
        private SdeFeatureClass()
        {

        }

        async static public Task<IFeatureClass> Create(SdeDataset dataset, string name, string multiVersionedViewName)
        {
            var featureClass = new SdeFeatureClass();

            if (dataset.RepoProvider == null)
            {
                throw new Exception("FeatureClass: Repository not initialized");
            }

            featureClass._name = name;

            (featureClass._geomType, featureClass._hasZ, featureClass._hasM) = await dataset.RepoProvider.FeatureClassGeometryType(featureClass);
            featureClass._fields = await dataset.RepoProvider.FeatureClassFields(featureClass);

            featureClass._idfield = featureClass._fields.ToEnumerable()
                .FirstOrDefault(f => f.type == FieldType.ID)?.name;

            featureClass._shapefield = featureClass._fields.ToEnumerable()
                .FirstOrDefault(f => f.type == FieldType.Shape)?.name;

            if (!String.IsNullOrEmpty(featureClass._shapefield))
            {
                switch (featureClass._geomType)
                {
                    case GeometryType.Polygon:
                        featureClass._fields.Add(new Field($"{featureClass._shapefield}.STArea()", FieldType.Double));
                        featureClass._fields.Add(new Field($"{featureClass._shapefield}.STLength()", FieldType.Double));
                        break;
                    case GeometryType.Polyline:
                        featureClass._fields.Add(new Field($"{featureClass._shapefield}.STLength()", FieldType.Double));
                        break;
                }
            }

            featureClass._dataset = dataset;

            if (featureClass._sRef == null && await dataset.RepoProvider.FeatureClassSpatialReference(featureClass) > 0)
            {
                featureClass._sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + await dataset.RepoProvider.FeatureClassSpatialReference(featureClass));
            }

            featureClass.MultiVersionedViewName = multiVersionedViewName;

            return featureClass;
        }

        public string MultiVersionedViewName { get; private set; }

        public override ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
        }

        #region IFeatureClassPerformanceInfo

        public bool SupportsHighperformanceOidQueries => String.IsNullOrEmpty(this.MultiVersionedViewName);

        #endregion
    }
}
