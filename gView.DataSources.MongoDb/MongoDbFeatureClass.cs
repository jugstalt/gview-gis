﻿using gView.Framework.Data;
using gView.Framework.Geometry;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MongoDb
{
    class MongoDbFeatureClass : IFeatureClass
    {
        private MongoDbFeatureClass()
        {

        }
        async static public Task<MongoDbFeatureClass> Create(MongoDbDataset dataset, Json.SpatialCollectionItem spatialCollectoinItem)
        {
            var fc = new MongoDbFeatureClass();

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

            fc.MongoCollection = dataset.GetFeatureCollection(fc.GeometryType);

            fc.Envelope = null;
            using (var cursor = new MongoDbFeatureCursor(fc, new QueryFilter()))
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

        public ISpatialReference SpatialReference { get; private set; }

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
            return ((MongoDbDataset)Dataset).Query(this, filter);
        }

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            return await ((MongoDbDataset)Dataset).Query(this, filter);
        }

        public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            throw new NotImplementedException();
        }

        #endregion

        internal IMongoCollection<Json.GeometryDocument> MongoCollection { get; private set; }

    }
}