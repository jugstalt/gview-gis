using gView.Framework.Data;
using gView.Framework.Geometry;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace gView.DataSources.CosmoDb.Json
{
    public class SpatialCollectionItem
    {
        public SpatialCollectionItem() { }

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

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("geometryDef")]
        public GeometryDefinition GeometryDef { get; set; }

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

        #region Classes

        public class GeometryDefinition
        {
            [JsonProperty("hasZ")]
            public bool HasZ { get; set; }

            [JsonProperty("hasM")]
            public bool HasM { get; set; }

            [JsonProperty("srefId")]
            public int SrefId { get; set; }

            [JsonProperty("geometryType")]
            public GeometryType GeometryType { get; set; }
        }

        public class Field
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public FieldType FieldType { get; set; }
        }

        #endregion
    }
}
