namespace gView.Framework.GeoJsonService.DTOs;
public enum Enums
{
    String,
    Integer,
    Float,
    Boolean,
    Date
}

public enum GeometryType
{
    Point,
    LineString,
    Polygon,
    MultiPoint,
    MultiLineString,
    MultiPolygon
}

public enum SpatialOperator
{
    Within,
    Intersects,
    Contains
}

public enum LogicOperator
{
    AND,
    OR,
    IN
}

public enum ComparisonOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    Like,
    In
}
