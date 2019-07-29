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
