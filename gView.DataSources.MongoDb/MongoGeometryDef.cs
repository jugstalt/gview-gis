using gView.Framework.Geometry;

namespace gView.DataSources.MongoDb
{
    public class MongoGeometryDef : GeometryDef
    {
        public MongoGeometryDef() { }

        public MongoGeometryDef(IGeometryDef geomDef)
        {
            this.GeometryType = geomDef.GeometryType;
            this.HasM = geomDef.HasM;
            this.HasZ = geomDef.HasZ;
            this.SpatialReference = geomDef.SpatialReference;
        }

        public int GeneralizationLevel { get; set; }
    }
}
