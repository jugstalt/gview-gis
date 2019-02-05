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
        public SdeFeatureClass(SdeDataset dataset, string name)
        {
            if (dataset.RepoProvider == null)
                throw new Exception("FeatureClass: Repository not initialized");

            _name = name;
            
            _geomType = dataset.RepoProvider.FeatureClassGeometryType(this);
            _fields = dataset.RepoProvider.FeatureClassFields(this);

            _idfield = _fields.ToEnumerable().Where(f => f.type == FieldType.ID).FirstOrDefault()?.name;
            _shapefield = _fields.ToEnumerable().Where(f => f.type == FieldType.Shape).FirstOrDefault()?.name;

            _dataset = dataset;

            if (_sRef == null && dataset.RepoProvider.FeatureClassSpatialReference(this)>0)
                _sRef = gView.Framework.Geometry.SpatialReference.FromID("epsg:" + dataset.RepoProvider.FeatureClassSpatialReference(this));
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
