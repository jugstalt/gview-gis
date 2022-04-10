// Only experimental Dataset
#pragma warning disable

using gView.Framework.Data;
using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using gView.Framework.Data.Metadata;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.MongoDb
{
    [RegisterPlugIn("8695CCB3-B8B4-476E-8011-F332494C4BFF")]
    public class MongoDbDataset : DatasetMetadata, IFeatureDataset2, IFeatureDatabase
    {
        private List<IDatasetElement> _layers;

        private const string SpatialCollectionRefName = "spatial_collections_ref";
        internal const string FeatureCollectionNamePrefix = "feature_collection_";

        private IMongoCollection<Json.SpatialCollectionItem> _spatialCollectionRef = null;

        #region IFeatureDataset

        public Task<int> CreateDataset(string name, ISpatialReference sRef)
        {
            throw new NotImplementedException();
        }

        public delegate IGeometryDef BeforeCreateFeatureClassHandler(string fcname, IGeometryDef geomDef);
        public BeforeCreateFeatureClassHandler BeforeCreateFeatureClass = null;

        async public Task<int> CreateFeatureClass(string dsname, string fcname, IGeometryDef geomDef, IFieldCollection fields)
        {
            try
            {
                if (_spatialCollectionRef == null)
                {
                    throw new Exception("No spatial collection reference. Open database before running CreateFeatureClass method");
                }

                var datasetElement = await Element(fcname);
                if (datasetElement != null)
                {
                    throw new Exception($"Featureclass {fcname} already exists");
                }

                if (BeforeCreateFeatureClass != null)
                {
                    geomDef = BeforeCreateFeatureClass(fcname, geomDef);
                }


                await _spatialCollectionRef.InsertOneAsync(
                    new Json.SpatialCollectionItem(geomDef, fields)
                    {
                        Name = fcname,
                        GeneralizationLevel = geomDef is MongoGeometryDef ?
                                ((MongoGeometryDef)geomDef).GeneralizationLevel :
                                -1
                    });

                return 1;
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastErrorMessage = ex.Message;

                return 0;
            }
        }

        public Task<IFeatureDataset> GetDataset(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDataset(string dsName)
        {
            throw new NotImplementedException();
        }

        async public Task<bool> DeleteFeatureClass(string fcName)
        {
            try
            {
                if (_spatialCollectionRef == null)
                {
                    throw new Exception("No spatial collection reference. Open database before running CreateFeatureClass method");
                }

                var datasetElement = await Element(fcName);
                if (datasetElement == null)
                {
                    throw new Exception($"Featureclass {fcName} not exists");
                }

                var mongoDatabase = GetMongoDatabase();
                if (await CollectionExistsAsync(mongoDatabase, fcName.ToFeatureClassCollectionName()))
                {
                    await GetMongoDatabase().DropCollectionAsync(fcName.ToFeatureClassCollectionName());
                }

                var deleteResult = await _spatialCollectionRef.DeleteOneAsync<Json.SpatialCollectionItem>(i => i.Name == fcName);
                return deleteResult.DeletedCount == 1;
            }
            catch (Exception ex)
            {
                this.LastException = ex;
                this.LastErrorMessage = ex.Message;
                return false;
            }
        }

        public Task<bool> RenameDataset(string name, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RenameFeatureClass(string name, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<IFeatureCursor> Query(IFeatureClass fc, IQueryFilter filter)
        {
            return Task.FromResult<IFeatureCursor>(new MongoDbFeatureCursor((MongoDbFeatureClass)fc, filter));
        }

        public Task<string[]> DatasetNames()
        {
            throw new NotImplementedException();
        }

        public bool Create(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Open(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Insert(IFeatureClass fClass, IFeature feature)
        {
            return Insert(fClass, new List<IFeature>(new IFeature[] { feature }));
        }

        async public Task<bool> Insert(IFeatureClass fClass, List<IFeature> features)
        {
            try
            {
                var featureCollection = await GetFeatureCollection(fClass);
                if (featureCollection == null)
                {
                    throw new Exception("No feature collection. Open database before running CreateFeatureClass method");
                }

                var spatialCollectionItem = await (await _spatialCollectionRef.FindAsync(s => s.Name == fClass.Name)).FirstOrDefaultAsync();
                if (spatialCollectionItem == null)
                {
                    throw new Exception("Feature class not exists");
                }

                IEnvelope bounds = spatialCollectionItem.FeatureBounds?.ToEnvelope();

                int fieldCount = fClass.Fields.Count;

                int degreeOfParallelism = 1;
                var containers = new List<Json.GeometryDocument>[degreeOfParallelism];
                for (int i = 0; i < degreeOfParallelism; i++)
                {
                    containers[i] = new List<Json.GeometryDocument>();
                }

                var generalizationLevel = fClass is MongoDbFeatureClass ?
                    ((MongoDbFeatureClass)fClass).GeneralizationLevel :
                    -1;

                int counter = 0;
                foreach (var feature in features)
                {
                    var document = new Json.GeometryDocument();
                    var documentProperties = new Dictionary<string, object>();

                    if (feature.Shape != null)
                    {
                        document.Shape = feature.Shape.ToGeoJsonGeometry<GeoJson2DGeographicCoordinates>();
                        if (bounds == null)
                        {
                            bounds = new Envelope(feature.Shape.Envelope);
                        }
                        else
                        {
                            bounds.Union(feature.Shape.Envelope);
                        }
                        if (fClass.GeometryType != GeometryType.Point)
                        {
                            document.Bounds = feature.Shape.Envelope.ToPolygon(0).ToGeoJsonGeometry<GeoJson2DGeographicCoordinates>();
                            document.AppendGeneralizedShapes(feature.Shape, fClass.SpatialReference, generalizationLevel);
                        }

                        #region Generalize

                        // dpm = 96/0.0254
                        // tol = pix * s / dpm
                        //
                        // pix = dpm/s   (tol=1) -> 1 Pixel sind beim 1:s pix[m]

                        #endregion
                    }

                    for (int f = 0; f < fieldCount; f++)
                    {
                        var field = fClass.Fields[f];
                        if (field.type == FieldType.ID ||
                            field.type == FieldType.Shape)
                        {
                            continue;
                        }

                        var fieldValue = feature.FindField(field.name);
                        if (fieldValue != null)
                        {
                            if (fieldValue.Value == DBNull.Value)
                            {
                                fieldValue.Value = null;
                            }

                            documentProperties.Add(fieldValue.Name, fieldValue.Value);
                        }
                    }

                    document.Properties = documentProperties;

                    containers[counter % degreeOfParallelism].Add(document);
                    counter++;
                }

                //Task[] tasks = new Task[degreeOfParallelism];
                //for (int i = 0; i < degreeOfParallelism; i++)
                //{
                //    tasks[i] = Task.Factory.StartNew(async (object index) =>
                //    {
                //        await featureCollection.InsertManyAsync(containers[(int)index]);
                //    }, (object)i);
                //}
                //await Task.WhenAll(tasks);

                await featureCollection.InsertManyAsync(containers[0]);

                var updateResult = await _spatialCollectionRef.UpdateOneAsync<Json.SpatialCollectionItem>(
                    c => c.Id == spatialCollectionItem.Id,
                    Builders<Json.SpatialCollectionItem>.Update.Set(i => i.FeatureBounds, new Json.SpatialCollectionItem.Bounds(bounds)));

                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastErrorMessage = ex.Message;

                return false;
            }
        }

        public Task<bool> Update(IFeatureClass fClass, IFeature feature)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(IFeatureClass fClass, List<IFeature> features)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(IFeatureClass fClass, int oid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(IFeatureClass fClass, string where)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IFeatureDataset2

        public string ConnectionString { get; private set; }

        public string DatasetGroupName => "CosmoDb Spatial Database";

        public string DatasetName { get; private set; }

        public string ProviderName => "gViewGIS";

        public DatasetState State { get; private set; }

        public string Query_FieldPrefix => String.Empty;

        public string Query_FieldPostfix => String.Empty;

        public IDatabase Database => this;

        public string LastErrorMessage { get; set; }

        public Exception LastException { get; private set; }

        public int SuggestedInsertFeatureCountPerTransaction => throw new NotImplementedException();

        public Task AppendElement(string elementName)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _spatialCollectionRef = null;
        }

        async public Task<IDatasetElement> Element(string title)
        {
            try
            {
                var collectionItem = await _spatialCollectionRef
                    .Find<Json.SpatialCollectionItem>(c => c.Name == title)
                    .FirstOrDefaultAsync();

                if (collectionItem == null)
                {
                    return null;
                }

                return new DatasetElement(await MongoDbFeatureClass.Create(this, collectionItem))
                {
                    Title = collectionItem.Name
                };
            }
            catch (Exception ex)
            {
                this.LastException = ex;
                return null;
            }
        }

        async public Task<List<IDatasetElement>> Elements()
        {
            if (_layers != null)
            {
                return _layers;
            }

            List<IDatasetElement> layers = new List<IDatasetElement>();
            foreach (var collectionItem in await _spatialCollectionRef.Find<Json.SpatialCollectionItem>(_ => true).ToListAsync())
            {
                layers.Add(new DatasetElement(await MongoDbFeatureClass.Create(this, collectionItem)));
            }

            _layers = layers;

            return _layers;
        }

        async public Task<IDataset2> EmptyCopy()
        {
            var dataset = new MongoDbDataset();
            await dataset.SetConnectionString(ConnectionString);
            await dataset.Open();

            return dataset;
        }

        public Task<IEnvelope> Envelope()
        {
            return Task.FromResult<IEnvelope>(new Envelope());
        }

        internal ISpatialReference _spatialReference = null;

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult<ISpatialReference>(_spatialReference);
        }

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
            if (_layers != null)
            {
                _layers.Clear();
            }

            await this.SetConnectionString((string)stream.Load("connectionstring", ""));
            return await this.Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.SaveEncrypted("connectionstring", this.ConnectionString);
        }

        async public Task<bool> Open()
        {
            try
            {
                var database = GetMongoDatabase();

                if (database == null)
                {
                    throw new Exception($"Database {ConnectionString.ExtractConnectionStringParameter("Database")} not exits");
                }

                if (!await CollectionExistsAsync(database, SpatialCollectionRefName))
                {
                    await database.CreateCollectionAsync(SpatialCollectionRefName);
                }

                _spatialCollectionRef = database.GetCollection<Json.SpatialCollectionItem>(SpatialCollectionRefName);

                _spatialReference = SpatialReference.FromID("epsg:4326");
            }
            catch (Exception ex)
            {
                this.LastErrorMessage = ex.Message;
                return false;
            }
            return true;
        }

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }

        public Task<bool> SetConnectionString(string connectionString)
        {
            this.ConnectionString = connectionString;
            return Task.FromResult(true);
        }

        public void SetSpatialReference(ISpatialReference sRef)
        {
            _spatialReference = sRef;
        }

        #endregion

        async internal Task<IMongoCollection<Json.GeometryDocument>> GetFeatureCollection(IFeatureClass fc)
        {
            var database = GetMongoDatabase();
            var collectionName = fc.Name.ToFeatureClassCollectionName();

            if (!await CollectionExistsAsync(database, collectionName))
            {
                return await CreateSpatialCollection(database, collectionName);
            }

            return database.GetCollection<Json.GeometryDocument>(collectionName);
        }

        #region Helper


        private IMongoDatabase GetMongoDatabase()
        {
            var client = new MongoClient(this.ConnectionString.ExtractConnectionStringParameter("Connection"));
            var database = client.GetDatabase(this.ConnectionString.ExtractConnectionStringParameter("Database"));

            return database;
        }

        private async Task<bool> CollectionExistsAsync(IMongoDatabase database, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }

        private async Task<IMongoCollection<Json.GeometryDocument>> CreateSpatialCollection(IMongoDatabase database, string collectionName)
        {
            #region Create Collection

            await database.CreateCollectionAsync(collectionName);
            var collection = database.GetCollection<Json.GeometryDocument>(collectionName);

            #endregion

            #region Create Spatial Index

            var spaitalIndex = Builders<Json.GeometryDocument>.IndexKeys.Geo2DSphere("_shape");
            await collection.Indexes.CreateOneAsync(spaitalIndex);

            spaitalIndex = Builders<Json.GeometryDocument>.IndexKeys.Geo2DSphere("_bounds");
            await collection.Indexes.CreateOneAsync(spaitalIndex);

            #endregion

            #region Create Shard Key

            //            var adminClient = new MongoClient(ConnectionString.ExtractConnectionStringParameter("Connection").Replace(":27017", ":27017"));

            //            var admin = adminClient.GetDatabase("admin");

            //            try
            //            {
            //                var shellCommandDb = new BsonDocument
            //                {
            //                    { "enableSharding", database.DatabaseNamespace.DatabaseName }
            //                };
            //                await admin.RunCommandAsync(new BsonDocumentCommand<BsonDocument>(shellCommandDb));
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine(ex.Message);
            //            }

            //            var partition = new BsonDocument {
            //                        {"shardCollection", $"{database.DatabaseNamespace.DatabaseName}.{collectionName}"},
            //                        {"key", new BsonDocument {{"_fc", "hashed"}}}
            //};
            //            var command = new BsonDocumentCommand<BsonDocument>(partition);
            //            await database.RunCommandAsync(command);

            #endregion

            return collection;
        }

        #endregion
    }
}
