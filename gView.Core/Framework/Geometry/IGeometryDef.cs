namespace gView.Framework.Geometry
{
    public interface IGeometryDef
    {
        bool HasZ { get; }
        bool HasM { get; }
        ISpatialReference SpatialReference { get;/* set;*/ }
        GeometryType GeometryType { get; }
        //int DigitAccuracy { get; }
        //gView.Framework.Data.GeometryFieldType GeometryFieldType { get; }
    }
}
