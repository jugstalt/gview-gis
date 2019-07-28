using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MongoDb
{
    [RegisterPlugIn("8695CCB3-B8B4-476E-8011-F332494C4BFF")]
    public class MongoDbDataset : DatasetMetadata, IFeatureDataset2, IFeatureDatabase
    {
        internal MongoClient _client = null;
        private List<IDatasetElement> _layers;

        private const string SpatialCollectionRefName = "spatial_collections_ref";
        private const string FeatureCollectionNamePoints = "feature_collection_points";
        private const string FeatureCollectionNameLines = "feature_collection_lines";
        private const string FeatureCollectionNamePolygons = "feature_collection_polygons";

        private IMongoCollection<Json.SpatialCollectionItem> _spatialCollectionRef = null;
        private IMongoCollection<Json.GeometryDocument> _featureCollection_points = null;
        private IMongoCollection<Json.GeometryDocument> _featureCollection_lines = null;
        private IMongoCollection<Json.GeometryDocument> _featureCollection_polygons = null;

        #region IFeatureDataset

        public Task<int> CreateDataset(string name, ISpatialReference sRef)
        {
            throw new NotImplementedException();
        }

        async public Task<int> CreateFeatureClass(string dsname, string fcname, IGeometryDef geomDef, IFields fields)
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

                var coordinates = new GeoJson2DGeographicCoordinates(15.0, 47.5);
                var jsonPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(coordinates);

                await _spatialCollectionRef.InsertOneAsync(
                    new Json.SpatialCollectionItem(geomDef, fields)
                    {
                        Name = fcname,
                        Test=jsonPoint
                    });

                return 1;
            }
            catch (Exception ex)
            {
                lastException = ex;
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

        public Task<bool> DeleteFeatureClass(string fcName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RenameDataset(string name, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RenameFeatureClass(string name, string newName)
        {
            throw new NotImplementedException();
        }

        async public Task<IFeatureCursor> Query(IFeatureClass fc, IQueryFilter filter)
        {
            return new MongoDbFeatureCursor((MongoDbFeatureClass)fc, filter);
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
                var featureCollection = GetFeatureCollection(fClass.GeometryType);
                if (featureCollection == null)
                {
                    throw new Exception("No feature collection. Open database before running CreateFeatureClass method");
                }

                int fieldCount = fClass.Fields.Count;

                int degreeOfParallelism = 1;
                var containers = new List<Json.GeometryDocument>[degreeOfParallelism];
                for (int i = 0; i < degreeOfParallelism; i++)
                {
                    containers[i] = new List<Json.GeometryDocument>();
                }

                int counter = 0;
                foreach (var feature in features)
                {
                    var document = new Json.GeometryDocument();
                    var documentDict = new Dictionary<string, object>();

                    document.FeatureClassName = fClass.Name;

                    if (feature.Shape != null)
                    {
                        document.Shape = feature.Shape.ToGeoJsonGeometry();
                    }
                    
                    //if (feature.Shape != null)
                    //{
                    //    expandoDict["_shape"] = MongoDB.Bson.poi feature.Shape.ToAzureGeometry();
                    //}
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
                            documentDict.Add(fieldValue.Name, fieldValue.Value);
                        }
                    }

                    //document.AddRange(documentDict);

                    containers[counter % degreeOfParallelism].Add(document);
                    //var result = await _client.CreateDocumentAsync(featureCollection.SelfLink, expandoObject);
                    counter++;
                }

                //Task[] tasks = new Task[5];
                //for (int i = 0; i < degreeOfParallelism; i++)
                //{
                //    tasks[i] = Task.Factory.StartNew(async (object index) =>
                //    {
                //        await featureCollection.InsertManyAsync(containers[(int)index]);
                //    }, (object)i);
                //}
                //await Task.WhenAll(tasks);

                await featureCollection.InsertManyAsync(containers[0]);

                return true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                LastErrorMessage = ex.Message;

                return true;
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

        public Exception lastException { get; private set; }

        public int SuggestedInsertFeatureCountPerTransaction => throw new NotImplementedException();

        async public Task AppendElement(string elementName)
        {

        }

        public void Dispose()
        {
            _featureCollection_points = null;
            _spatialCollectionRef = null;
            _client = null;
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
                this.lastException = ex;
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

        async public Task<IEnvelope> Envelope()
        {
            return new Envelope();
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
                _client = new MongoDB.Driver.MongoClient(ConnectionString.ExtractConnectionStringParameter("Connection"));
                var database = _client.GetDatabase(ConnectionString.ExtractConnectionStringParameter("Database"));

                if (database == null)
                {
                    throw new Exception($"Database {ConnectionString.ExtractConnectionStringParameter("Database")} not exits");
                }

                _spatialCollectionRef = database.GetCollection<Json.SpatialCollectionItem>(SpatialCollectionRefName);

                #region Points Collection

                _featureCollection_points = database.GetCollection<Json.GeometryDocument>(FeatureCollectionNamePoints);

                #endregion

                #region Lines Collection

                _featureCollection_lines = database.GetCollection<Json.GeometryDocument>(FeatureCollectionNameLines);

                #endregion

                #region Polygons Collection

                _featureCollection_polygons = database.GetCollection<Json.GeometryDocument>(FeatureCollectionNamePolygons);

                #endregion

                _spatialReference = SpatialReference.FromID("epsg:4326");
            }
            catch (Exception ex)
            {
                this.LastErrorMessage = ex.Message;
                return false;
            }
            return true;
        }

        async public Task RefreshClasses()
        {

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

        internal IMongoCollection<Json.GeometryDocument> GetFeatureCollection(geometryType geomType)
        {
            switch (geomType)
            {
                case geometryType.Point:
                case geometryType.Multipoint:
                    return _featureCollection_points;
                case geometryType.Polyline:
                    return _featureCollection_lines;
                case geometryType.Polygon:
                    return _featureCollection_polygons;
                default:
                    throw new Exception($"There is no collection for geometry-type '{geomType.ToString()}'");
            }
        }
    }
}
