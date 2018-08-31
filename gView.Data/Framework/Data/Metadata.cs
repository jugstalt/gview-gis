using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.IO;
using gView.Framework.Carto;
using gView.Framework.system;
using System.Xml;

namespace gView.Framework.Data
{
    public class Metadata : IMetadata
    {
        private List<IMetadataProvider> _providers = null;

        #region IMetadata Member

        virtual public void ReadMetadata(IPersistStream stream)
        {
            _providers = new List<IMetadataProvider>();
            IMetadataProvider provider = null;
            while ((provider = (IMetadataProvider)stream.Load("IMetadataProvider", null, this)) != null)
            {
                _providers.Add(provider);
            }
        }

        virtual public void WriteMetadata(IPersistStream stream)
        {
            PlugInManager plugins = new PlugInManager();

            if (_providers != null)
            {
                foreach (IMetadataProvider provider in _providers)
                {
                    if (provider == null) continue;

                    // mit ApplyTo noch einmal das this Objekt auf den Provider 
                    // setzen, damit beim speichern immer das aktuelle Object gesetzt wird...
                    provider.ApplyTo(this);
                    stream.Save("IMetadataProvider", provider);
                }
            }
            else
            {
                _providers = new List<IMetadataProvider>();
            }

            foreach (XmlNode providerNode in plugins.GetPluginNodes(Plugins.Type.IMetadataProvider))
            {
                IMetadataProvider provider = plugins.CreateInstance(providerNode) as IMetadataProvider;
                if (provider == null) continue;

                // nach bereits vorhanden suchen...
                IMetadataProvider provider2 = this.MetadataProvider(PlugInManager.PlugInID(provider));
                if (provider2 != null)
                    continue;

                if (provider.ApplyTo(this))
                {
                    stream.Save("IMetadataProvider", provider);
                    _providers.Add(provider);
                }
            }
        }

        public IMetadataProvider MetadataProvider(Guid guid)
        {
            if (_providers == null) return null;

            foreach (IMetadataProvider provider in _providers)
            {
                if (PlugInManager.PlugInID(provider).Equals(guid))
                    return provider;
            }
            return null;
        }

        public List<IMetadataProvider> Providers
        {
            get
            {
                if (_providers == null) return new List<IMetadataProvider>();

                return ListOperations<IMetadataProvider>.Clone(_providers);
            }

            set
            {
                if (value == null)
                    _providers = null;
                _providers = ListOperations<IMetadataProvider>.Clone(value);

                foreach (IMetadataProvider provider in _providers)
                    if (provider != null)
                        provider.ApplyTo(this); 
            }
        }
        #endregion
    }

    public class MapMetadata : Metadata
    {
        public override void ReadMetadata(IPersistStream stream)
        {
            if (!(this is IMap)) return;
            IMap map = (IMap)this;

            base.ReadMetadata(stream);

            //XmlStreamObject streamObject;
            //while ((streamObject = (XmlStreamObject)stream.Load("MetaNode", null)) != null)
            //{
            //}
        }

        public override void WriteMetadata(IPersistStream stream)
        {
            if (!(this is IMap)) return;
            IMap map = (IMap)this;

            base.WriteMetadata(stream);

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
                if (dataset == null) break;

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
                if (_datasets == null) return;

                string ConnectionString = (string)stream.Load("ConnectionString","");
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
                if (_dataset == null) return;

                stream.Save("ConnectionString", _dataset.ConnectionString);
                stream.Save("DatasetID", _datasetID);

                _dataset.WriteMetadata(stream);
            }

            #endregion
        }
        #endregion
    }

    public class DatasetMetadata : Metadata
    {
    }

    public class DatasetElementMetadata : Metadata
    {
    }

    public class FieldMetadata : Metadata
    {
    }
}
