using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Data
{
    public enum FeatureLayerCompositionMode 
    {
        Over = 0,
        Copy = 1
    }

    public enum spatialRelation
    {
        SpatialRelationMapEnvelopeIntersects = 0,
        SpatialRelationIntersects = 1,
        SpatialRelationEnvelopeIntersects = 2,
        SpatialRelationWithin = 3,
        SpatialRelationContains = 4
    }
}
