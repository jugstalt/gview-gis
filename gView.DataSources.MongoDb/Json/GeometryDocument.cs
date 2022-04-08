using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Collections.Generic;

namespace gView.DataSources.MongoDb.Json
{
    public class GeometryDocument /*: BsonDocument*/
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("_shape")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> Shape { get; set; }

        [BsonElement("_bounds")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> Bounds { get; set; }

        [BsonElement("_shapeGen0")]
        public byte[] ShapeGeneralized0 { get; set; }
        [BsonElement("_shapeGen1")]
        public byte[] ShapeGeneralized1 { get; set; }
        [BsonElement("_shapeGen2")]
        public byte[] ShapeGeneralized2 { get; set; }
        [BsonElement("_shapeGen3")]
        public byte[] ShapeGeneralized3 { get; set; }
        [BsonElement("_shapeGen4")]
        public byte[] ShapeGeneralized4 { get; set; }
        [BsonElement("_shapeGen5")]
        public byte[] ShapeGeneralized5 { get; set; }
        [BsonElement("_shapeGen6")]
        public byte[] ShapeGeneralized6 { get; set; }
        [BsonElement("_shapeGen7")]
        public byte[] ShapeGeneralized7 { get; set; }
        [BsonElement("_shapeGen8")]
        public byte[] ShapeGeneralized8 { get; set; }
        [BsonElement("_shapeGen9")]
        public byte[] ShapeGeneralized9 { get; set; }
        [BsonElement("_shapeGen10")]
        public byte[] ShapeGeneralized10 { get; set; }
        [BsonElement("_shapeGen11")]
        public byte[] ShapeGeneralized11 { get; set; }
        [BsonElement("_shapeGen12")]
        public byte[] ShapeGeneralized12 { get; set; }
        [BsonElement("_shapeGen13")]
        public byte[] ShapeGeneralized13 { get; set; }
        [BsonElement("_shapeGen14")]
        public byte[] ShapeGeneralized14 { get; set; }

        [BsonElement("properties")]
        public Dictionary<string, object> Properties { get; set; }
    }

    //public class PointDocument : BsonDocument
    //{
    //    [BsonId]
    //    public ObjectId Id { get; set; }

    //    [BsonElement("_shape")]
    //    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Shape { get; set; }
    //}

    //public class LineDocument : BsonDocument
    //{
    //    [BsonId]
    //    public ObjectId Id { get; set; }

    //    [BsonElement("_shape")]
    //    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Shape { get; set; }
    //}

    //public class PolygonDocument : BsonDocument
    //{
    //    [BsonId]
    //    public ObjectId Id { get; set; }

    //    [BsonElement("_shape")]
    //    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Shape { get; set; }
    //}
}
