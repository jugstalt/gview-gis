using gView.Framework.Data;
using gView.Framework.Geometry;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace gView.DataSources.CosmoDb
{
    class CosmoDbFeatureClass : IFeatureClass
    {
        private CosmoDbFeatureClass()
        {

        }
        async static public Task<CosmoDbFeatureClass> Create(CosmoDbDataset dataset, Json.SpatialCollectionItem spatialCollectoinItem)
        {
            var fc = new CosmoDbFeatureClass();

            fc.Dataset = dataset;
            fc.Name = spatialCollectoinItem.Name;

            if (spatialCollectoinItem.GeometryDef != null)
            {
                fc.HasZ = spatialCollectoinItem.GeometryDef.HasZ;
                fc.HasM = spatialCollectoinItem.GeometryDef.HasM;
                fc.GeometryType = spatialCollectoinItem.GeometryDef.GeometryType;
                fc.SpatialReference = dataset._spatialReference;  // ToDo
            }

            var fields = new Fields();
            if (spatialCollectoinItem.Fields != null)
            {
                foreach (var field in spatialCollectoinItem.Fields)
                {
                    fields.Add(new Field()
                    {
                        name = field.Name,
                        type = field.FieldType
                    });
                }
            }
            fc.Fields = fields;

            fc.CosmoDocumentClient = dataset._client;
            fc.CosmoDocumentCollection = dataset.GetFeatureCollection(fc.GeometryType);

            fc.Envelope = null;
            using (var cursor = new CosmoDbFeatureCursor(fc, new QueryFilter()))
            {
                IFeature feature = null;
                while ((feature = await cursor.NextFeature()) != null)
                {
                    if (feature.Shape == null)
                    {
                        continue;
                    }

                    if (fc.Envelope == null)
                    {
                        fc.Envelope = feature.Shape.Envelope;
                    }
                    else
                    {
                        fc.Envelope.Union(feature.Shape.Envelope);
                    }
                }
            }

            return fc;
        }

        #region IFeatureClass

        public string ShapeFieldName => "_shape";

        public IEnvelope Envelope { get; private set; }

        public IFields Fields { get; private set; }

        public string IDFieldName => "id";

        public string Name { get; private set; }

        public string Aliasname => Name;

        public IDataset Dataset { get; private set; }

        public bool HasZ { get; private set; }

        public bool HasM { get; private set; }

        public ISpatialReference SpatialReference { get; set; }

        public geometryType GeometryType { get; private set; }

        public Task<int> CountFeatures()
        {
            return null;
        }

        public IField FindField(string name)
        {
            return Fields.FindField(name);
        }

        public Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
        {
            return ((CosmoDbDataset)Dataset).Query(this, filter);
        }

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            return await ((CosmoDbDataset)Dataset).Query(this, filter);
        }

        public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            throw new NotImplementedException();
        }

        #endregion

        internal DocumentClient CosmoDocumentClient { get; private set; }
        internal DocumentCollection CosmoDocumentCollection { get; private set; }

    }
}
