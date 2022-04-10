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

    public enum FieldType
    {
        ID = 0,
        Shape = 1,
        boolean = 2,
        biginteger = 3,
        character = 4,
        integer = 5,
        smallinteger = 6,
        Float = 7,
        Double = 8,
        String = 9,
        Date = 10,
        unknown = 11,
        binary = 12,
        guid = 13,
        replicationID = 14,
        GEOMETRY = 15,
        GEOGRAPHY = 16,
        NString = 17
    }

    public enum GeometryFieldType
    {
        Default = FieldType.Shape,
        MsGeometry = FieldType.GEOMETRY,
        MsGeography = FieldType.GEOGRAPHY
    }
}
