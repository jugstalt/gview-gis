using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Data.Metadata;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.CosmoDb
{
    //[RegisterPlugIn("8119C1CA-B8E1-4C55-B828-922966A56540")]
    public class CosmoDbDataset : DatasetMetadata, IFeatureDataset2, IFeatureDatabase
    {
        internal DocumentClient _client = null;
        private List<IDatasetElement> _layers;

        private const string SpatialCollectionRefName = "spatial_collections_ref";
        private const string FeatureCollectionNamePoints = "feature_collection_points";
        private const string FeatureCollectionNameLines = "feature_collection_lines";
        private const string FeatureCollectionNamePolygons = "feature_collection_polygons";

        private DocumentCollection _spatialCollectionRef = null;
        private DocumentCollection
            _featureCollection_points = null,
            _featureCollection_lines = null,
            _featureCollection_polygons = null;

        #region IFeatureDataset

        public Task<int> CreateDataset(string name, ISpatialReference sRef)
        {
            throw new NotImplementedException();
        }

        async public Task<int> CreateFeatureClass(string dsname, string fcname, IGeometryDef geomDef, IFieldCollection fields)
        {
            try
            {
                if (_client == null || _spatialCollectionRef == null)
                {
                    throw new Exception("No spatial collection reference. Open database before running CreateFeatureClass method");
                }

                var datasetElement = await Element(fcname);
                if (datasetElement != null)
                {
                    throw new Exception($"Featureclass {fcname} already exists");
                }

                var result = await _client.CreateDocumentAsync(_spatialCollectionRef.SelfLink,
                    new Json.SpatialCollectionItem(geomDef, fields)
                    {
                        Name = fcname
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

        public Task<IFeatureCursor> Query(IFeatureClass fc, IQueryFilter filter)
        {
            return Task.FromResult<IFeatureCursor>(new CosmoDbFeatureCursor((CosmoDbFeatureClass)fc, filter));
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
                if (_client == null || featureCollection == null)
                {
                    throw new Exception("No feature collection. Open database before running CreateFeatureClass method");
                }

                int fieldCount = fClass.Fields.Count;

                var documentCollection = new DocumentCollection();

                int degreeOfParallelism = 5;
                var containers = new List<ExpandoObject>[degreeOfParallelism];
                for (int i = 0; i < degreeOfParallelism; i++)
                {
                    containers[i] = new List<ExpandoObject>();
                }

                int counter = 0;
                foreach (var feature in features)
                {
                    var expandoObject = new ExpandoObject();
                    var expandoDict = (IDictionary<string, object>)expandoObject;
                    expandoDict["_fc"] = fClass.Name;

                    if (feature.Shape != null)
                    {
                        expandoDict["_shape"] = feature.Shape.ToAzureGeometry();
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
                            expandoDict[fieldValue.Name] = fieldValue.Value;
                        }
                    }

                    containers[counter % degreeOfParallelism].Add(expandoObject);
                    //var result = await _client.CreateDocumentAsync(featureCollection.SelfLink, expandoObject);
                    counter++;
                }

                Task[] tasks = new Task[5];
                for (int i = 0; i < degreeOfParallelism; i++)
                {
                    tasks[i] = Task.Factory.StartNew(async (object index) =>
                    {
                        foreach (var expandoObjet in containers[(int)index])
                        {
                            var result = await _client.CreateDocumentAsync(featureCollection.SelfLink, expandoObjet);
                        }
                    }, (object)i);
                }
                await Task.WhenAll(tasks);

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
            _featureCollection_points = _spatialCollectionRef = null;
            _client.Dispose();
        }

        async public Task<IDatasetElement> Element(string title)
        {
            var result = _client.CreateDocumentQuery<Json.SpatialCollectionItem>(_spatialCollectionRef.SelfLink)
                .Where(d => d.Name == title)
                .AsEnumerable<Json.SpatialCollectionItem>()
                .FirstOrDefault();

            if (result == null)
            {
                return null;
            }

            return new DatasetElement(await CosmoDbFeatureClass.Create(this, result))
            {
                Title = result.Name
            };
        }

        async public Task<List<IDatasetElement>> Elements()
        {
            if (_layers != null)
            {
                return _layers;
            }

            List<IDatasetElement> layers = new List<IDatasetElement>();
            foreach (var collectionItem in _client.CreateDocumentQuery<Json.SpatialCollectionItem>(_spatialCollectionRef.SelfLink).AsEnumerable())
            {
                layers.Add(new DatasetElement(await CosmoDbFeatureClass.Create(this, collectionItem)));
            }

            _layers = layers;

            return _layers;
        }

        async public Task<IDataset2> EmptyCopy()
        {
            var dataset = new CosmoDbDataset();
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
                _client = new DocumentClient(
                    new Uri(ConnectionString.ExtractConnectionStringParameter("AccountEndpoint")),
                    ConnectionString.ExtractConnectionStringParameter("AccountKey"));

                var database = _client.CreateDatabaseQuery()
                    .Where(db => db.Id == ConnectionString.ExtractConnectionStringParameter("Database"))
                    .AsEnumerable()
                    .FirstOrDefault();

                if (database == null)
                {
                    throw new Exception($"Database {ConnectionString.ExtractConnectionStringParameter("Database")} not exits");
                }

                _spatialCollectionRef = _client.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(col => col.Id == SpatialCollectionRefName)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (_spatialCollectionRef == null)
                {
                    var partitionKey = new PartitionKeyDefinition();
                    partitionKey.Paths.Add("/name");

                    var response = await _client.CreateDocumentCollectionAsync(
                        database.SelfLink,
                        new DocumentCollection()
                        {
                            Id = SpatialCollectionRefName,
                            PartitionKey = partitionKey
                        });

                    _spatialCollectionRef = response.Resource;
                }

                #region Points Collection

                _featureCollection_points = _client.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(col => col.Id == FeatureCollectionNamePoints)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (_featureCollection_points == null)
                {
                    IndexingPolicy indexingPolicyWithSpatialEnabled = new IndexingPolicy
                    {
                        IncludedPaths = new System.Collections.ObjectModel.Collection<IncludedPath>()
                        {
                            new IncludedPath
                            {
                                Path = "/*",
                                Indexes = new System.Collections.ObjectModel.Collection<Index>()
                                {
                                    new SpatialIndex(DataType.Point),
                                    new RangeIndex(DataType.Number) { Precision = -1 },
                                    new RangeIndex(DataType.String) { Precision = -1 }
                                }
                            }
                        }
                    };

                    var partitionKey = new PartitionKeyDefinition();
                    partitionKey.Paths.Add("/_fc");


                    var response = await _client.CreateDocumentCollectionAsync(
                        database.SelfLink,
                        new DocumentCollection()
                        {
                            Id = FeatureCollectionNamePoints,
                            PartitionKey = partitionKey,
                            IndexingPolicy = indexingPolicyWithSpatialEnabled
                        });

                    // ToDo: Create Spatial Index

                    _featureCollection_points = response.Resource;
                }

                #endregion

                #region Lines Collection

                _featureCollection_lines = _client.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(col => col.Id == FeatureCollectionNameLines)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (_featureCollection_lines == null)
                {
                    IndexingPolicy indexingPolicyWithSpatialEnabled = new IndexingPolicy
                    {
                        IncludedPaths = new System.Collections.ObjectModel.Collection<IncludedPath>()
                        {
                            new IncludedPath
                            {
                                Path = "/*",
                                Indexes = new System.Collections.ObjectModel.Collection<Index>()
                                {
                                    new SpatialIndex(DataType.LineString),
                                    new RangeIndex(DataType.Number) { Precision = -1 },
                                    new RangeIndex(DataType.String) { Precision = -1 }
                                }
                            }
                        }
                    };

                    var partitionKey = new PartitionKeyDefinition();
                    partitionKey.Paths.Add("/_fc");


                    var response = await _client.CreateDocumentCollectionAsync(
                        database.SelfLink,
                        new DocumentCollection()
                        {
                            Id = FeatureCollectionNameLines,
                            PartitionKey = partitionKey,
                            IndexingPolicy = indexingPolicyWithSpatialEnabled
                        });

                    _featureCollection_lines = response.Resource;
                }

                #endregion

                #region Polygons Collection

                _featureCollection_polygons = _client.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(col => col.Id == FeatureCollectionNamePolygons)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (_featureCollection_polygons == null)
                {
                    IndexingPolicy indexingPolicyWithSpatialEnabled = new IndexingPolicy
                    {
                        IncludedPaths = new System.Collections.ObjectModel.Collection<IncludedPath>()
                        {
                            new IncludedPath
                            {
                                Path = "/*",
                                Indexes = new System.Collections.ObjectModel.Collection<Index>()
                                {
                                    new SpatialIndex(DataType.Point),
                                    new SpatialIndex(DataType.Polygon),
                                    new RangeIndex(DataType.Number) { Precision = -1 },
                                    new RangeIndex(DataType.String) { Precision = -1 }
                                }
                            }
                        }
                    };

                    var partitionKey = new PartitionKeyDefinition();
                    partitionKey.Paths.Add("/_fc");


                    var response = await _client.CreateDocumentCollectionAsync(
                        database.SelfLink,
                        new DocumentCollection()
                        {
                            Id = FeatureCollectionNamePolygons,
                            PartitionKey = partitionKey,
                            IndexingPolicy = indexingPolicyWithSpatialEnabled
                        });

                    _featureCollection_polygons = response.Resource;
                }

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

        internal DocumentCollection GetFeatureCollection(GeometryType geomType)
        {
            switch (geomType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return _featureCollection_points;
                case GeometryType.Polyline:
                    return _featureCollection_lines;
                case GeometryType.Polygon:
                    return _featureCollection_polygons;
                default:
                    throw new Exception($"There is no collection for geometry-type '{geomType.ToString()}'");
            }
        }
    }
}
