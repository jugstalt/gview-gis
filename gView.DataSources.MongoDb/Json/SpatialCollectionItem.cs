using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace gView.DataSources.MongoDb.Json
{
    public class SpatialCollectionItem
    {
        public SpatialCollectionItem()
        {
            this.GeneralizationLevel = 0;
        }

        public SpatialCollectionItem(IGeometryDef geomDef, IFieldCollection fields)
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

        private void AddFields(IFieldCollection fields)
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

        [BsonElement("bounds")]
        public Bounds FeatureBounds { get; set; }

        [BsonElement("generalizationLevel")]
        public int GeneralizationLevel { get; set; }

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
            public GeometryType GeometryType { get; set; }
        }

        public class Field
        {
            [BsonElement("name")]
            public string Name { get; set; }

            [BsonElement("type")]
            public FieldType FieldType { get; set; }
        }

        public class Bounds
        {
            public Bounds() { }
            public Bounds(IEnvelope envelpe)
            {
                this.MinX = envelpe.minx;
                this.MinY = envelpe.miny;
                this.MaxX = envelpe.maxx;
                this.MaxY = envelpe.maxy;
            }

            [BsonElement("minx")]
            double MinX { get; set; }

            [BsonElement("miny")]
            double MinY { get; set; }

            [BsonElement("maxx")]
            double MaxX { get; set; }

            [BsonElement("maxy")]
            double MaxY { get; set; }

            public IEnvelope ToEnvelope()
            {
                return new Envelope(MinX, MinY, MaxX, MaxY);
            }
        }

        #endregion
    }
}
