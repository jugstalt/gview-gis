using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde
{
    public class SdeFeatureClass : gView.Framework.OGC.DB.OgcSpatialFeatureclass
    {
        private SdeFeatureClass()
        {
            
        }

        async static public Task<IFeatureClass> Create(SdeDataset dataset, string name)
        {
            var featureClass = new SdeFeatureClass();

            if (dataset.RepoProvider == null)
                throw new Exception("FeatureClass: Repository not initialized");

            featureClass._name = name;

            featureClass._geomType = await dataset.RepoProvider.FeatureClassGeometryType(featureClass);
            featureClass._fields = await dataset.RepoProvider.FeatureClassFields(featureClass);

            featureClass._idfield = featureClass._fields.ToEnumerable()
                .Where(f => f.type == FieldType.ID)
                .FirstOrDefault()?.name;

            featureClass._shapefield = featureClass._fields.ToEnumerable()
                .Where(f => f.type == FieldType.Shape)
                .FirstOrDefault()?.name;

            featureClass._dataset = dataset;

            if (featureClass._sRef == null && await dataset.RepoProvider.FeatureClassSpatialReference(featureClass) > 0)
                featureClass._sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + dataset.RepoProvider.FeatureClassSpatialReference(featureClass));

            return featureClass;
        }

        public override ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
        }
    }
}
