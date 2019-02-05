using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.OGC.Framework.OGC.DB
{
    public interface IRepoProvider
    {
        IEnvelope FeatureClassEnveolpe(IFeatureClass fc);

        int FeatureClassSpatialReference(IFeatureClass fc);

        Fields FeatureClassFields(IFeatureClass fc);

        geometryType FeatureClassGeometryType(IFeatureClass fc);
    }
}
