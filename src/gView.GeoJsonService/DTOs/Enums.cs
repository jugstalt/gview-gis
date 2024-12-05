namespace gView.GeoJsonService.DTOs;

public enum PropertyType
{
    String,
    Integer,
    Float,
    Boolean,
    Date
}

public enum GeometryType
{
    Unknown,
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
    Not_Equals,
    Greater_Than,
    Greater_Or_Equal_Than,
    Less_Than,
    Less_Or_Equal_Than,
    Like,
    In
}

public enum QueryCommand
{
    Select,
    CountOnly,
    Distinct,
    IdsOnly
}

public enum GeometryResult
{
    None,
    Geometry,
    BBox
}

public enum MapReponseFormat
{
    Link,
    Base64,
    Image
}
