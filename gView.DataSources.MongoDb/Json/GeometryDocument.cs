using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

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
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized0 { get; set; }
        [BsonElement("_shapeGen1")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized1 { get; set; }
        [BsonElement("_shapeGen2")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized2 { get; set; }
        [BsonElement("_shapeGen3")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized3 { get; set; }
        [BsonElement("_shapeGen4")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized4 { get; set; }
        [BsonElement("_shapeGen5")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized5 { get; set; }
        [BsonElement("_shapeGen6")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized6 { get; set; }
        [BsonElement("_shapeGen7")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized7 { get; set; }
        [BsonElement("_shapeGen8")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized8 { get; set; }
        [BsonElement("_shapeGen9")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized9 { get; set; }
        [BsonElement("_shapeGen10")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized10 { get; set; }
        [BsonElement("_shapeGen11")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized11 { get; set; }
        [BsonElement("_shapeGen12")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized12 { get; set; }
        [BsonElement("_shapeGen13")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized13 { get; set; }
        [BsonElement("_shapeGen14")]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ShapeGeneralized14 { get; set; }

        [BsonElement("properties")]
        public Dictionary<string,object> Properties { get; set; }
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
