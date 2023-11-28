using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Data.Metadata
{
    public class MapMetadata : Metadata
    {
        public override void ReadMetadata(IPersistStream stream)
        {
            if (!(this is IMap))
            {
                return;
            }

            IMap map = (IMap)this;

            base.ReadMetadata(stream);

            //XmlStreamObject streamObject;
            //while ((streamObject = (XmlStreamObject)stream.Load("MetaNode", null)) != null)
            //{
            //}
        }

        async public override Task WriteMetadata(IPersistStream stream)
        {
            if (!(this is IMap))
            {
                return;
            }

            IMap map = (IMap)this;

            await base.WriteMetadata(stream);

            //int index = 0;
            //while (true)
            //{
            //    IDataset dataset = map[index++];
            //    if (dataset == null) break;

            //    IPersistableDictionary dictionary = PlugInManager.Create(KnownObjects.Persistable_Dictionary) as IPersistableDictionary;
            //    IPersistable metaPersist = PlugInManager.Create(KnownObjects.Persistable_Metadata, dataset) as IPersistable;
            //    if (dictionary == null || metaPersist == null) break;

            //    dictionary["ConnectionString"] = dataset.ConnectionString;
            //    dictionary["DatasetID"] = index - 1;
            //    dictionary["Metadata"] = metaPersist;

            //    XmlStreamObject datasetObject = new XmlStreamObject(dictionary);
            //    datasetObject.Category = "Datasets";
            //    datasetObject.DisplayName = dataset.DatasetName;
            //    stream.Save("MetaNode", datasetObject);

            //    //stream.Save("Dataset", new DatasetMetadataWriter(dataset, index - 1));
            //}
        }

        #region Helper
        private List<IDataset> MapDatasets(IMap map)
        {
            List<IDataset> datasets = new List<IDataset>();

            int index = 0;
            while (true)
            {
                IDataset dataset = map[index++];
                if (dataset == null)
                {
                    break;
                }

                datasets.Add(dataset);
            }
            return datasets;
        }
        #endregion

        #region Classes
        private class DatasetMetadataReader : IPersistable
        {
            public List<IDataset> _datasets;

            public DatasetMetadataReader(List<IDataset> datasets)
            {
                _datasets = datasets;
            }
            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                if (_datasets == null)
                {
                    return;
                }

                string ConnectionString = (string)stream.Load("ConnectionString", "");
                int datasetID = (int)stream.Load("DatasetID", -1);

                if (datasetID >= 0 && datasetID < _datasets.Count &&
                    _datasets[datasetID].ConnectionString == ConnectionString)
                {
                    _datasets[datasetID].ReadMetadata(stream);
                }
            }

            public void Save(IPersistStream stream)
            {
            }

            #endregion
        }

        private class DatasetMetadataWriter : IPersistable
        {
            IDataset _dataset;
            int _datasetID;
            public DatasetMetadataWriter(IDataset dataset, int datasetID)
            {
                _dataset = dataset;
                _datasetID = datasetID;
            }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
            }
            public void Save(IPersistStream stream)
            {
                if (_dataset == null)
                {
                    return;
                }

                stream.Save("ConnectionString", _dataset.ConnectionString);
                stream.Save("DatasetID", _datasetID);

                _dataset.WriteMetadata(stream);

                return;
            }

            #endregion
        }
        #endregion
    }
}
