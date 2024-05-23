using gView.GraphicsEngine;

namespace gView.Framework.Core.Data
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

    public enum DatasetState { unknown = 0, opened = 1 }

    public enum DatasetNameCase { ignore = 0, upper = 1, lower = 2, classNameUpper = 3, classNameLower = 4, fieldNamesUpper = 5, fieldNamesLower = 6 }

    public enum MapServerGrouplayerStyle
    {
        Dropdownable = 0,
        Checkbox = 1
    }

    public enum InterpolationMethod
    {
        Fast = InterpolationMode.Low,
        NearestNeighbor = InterpolationMode.NearestNeighbor,
        Bilinear = InterpolationMode.Bilinear,
        Bicubic = InterpolationMode.Bicubic /*,
        HighQuality = System.Drawing.Drawing2D.InterpolationMode.High */
    }

    public enum TileGridType
    {
        image_jpg = 0,
        image_png = 1,
        binary_float = 2
    }
    public enum TileLevelType
    {
        ConstantImagesize = 0,
        ConstantGeographicTilesize = 1
    }

    public enum GridRenderMethode { None = 0, Colors = 1, HillShade = 4, NullValue = 8 }

    public enum joinType
    {
        LeftOuterJoin = 0,
        LeftInnerJoin = 1
    }

    public enum CombinationMethod { New, Union, Intersection, Difference, SymDifference }
}
