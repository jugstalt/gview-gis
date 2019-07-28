using gView.Framework.Data;
using gView.Framework.Geometry;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace gView.DataSources.MongoDb.Json
{
    public class SpatialCollectionItem
    {
        public SpatialCollectionItem() { }

        public SpatialCollectionItem(IGeometryDef geomDef, IFields fields)
        {
            AddFields(fields);
            this.GeometryDef = new GeometryDefinition()
            {
                HasM = geomDef.HasM,
                HasZ = geomDef.HasZ,
                GeometryType = geomDef.GeometryType,
                SrefId = 4326
            };
        }

        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("geometryDef")]
        public GeometryDefinition GeometryDef { get; set; }

        [BsonElement("fields")]
        public IEnumerable<Field> Fields { get; set; }

        private void AddFields(IFields fields)
        {
            if (fields == null)
            {
                return;
            }

            List<Field> fieldCollection = new List<Field>();

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                if (field.type == FieldType.ID ||
                    field.type == FieldType.Shape)
                {
                    continue;
                }

                fieldCollection.Add(new Field()
                {
                    Name = field.name,
                    FieldType = field.type
                });
            }

            this.Fields = fieldCollection;
        }

        [BsonElement("test")]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Test { get; set; }

        #region Classes

        public class GeometryDefinition
        {
            [BsonElement("hasZ")]
            public bool HasZ { get; set; }

            [BsonElement("hasM")]
            public bool HasM { get; set; }

            [BsonElement("srefId")]
            public int SrefId { get; set; }

            [BsonElement("geometryType")]
            public geometryType GeometryType { get; set; }
        }

        public class Field
        {
            [BsonElement("name")]
            public string Name { get; set; }

            [BsonElement("type")]
            public FieldType FieldType { get; set; }
        }

        #endregion
    }
}
