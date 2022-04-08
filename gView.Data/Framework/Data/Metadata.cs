using gView.Framework.Carto;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public class Metadata : IMetadata
    {
        private ConcurrentBag<IMetadataProvider> _providers = null;

        #region IMetadata Member

        virtual public void ReadMetadata(IPersistStream stream)
        {
            var providers = new List<IMetadataProvider>();
            IMetadataProvider provider = null;

            while ((provider = (IMetadataProvider)stream.Load("IMetadataProvider", null, this)) != null)
            {
                providers.Add(provider);
            }

            // Append Exising Providers
            if (_providers != null)
            {
                foreach (var existingProvider in _providers)
                {
                    if (providers.Where(p => p.GetType().Equals(existingProvider.GetType())).Count() == 0)
                    {
                        providers.Add(existingProvider);
                    }
                }
            }

            _providers = new ConcurrentBag<IMetadataProvider>(providers);
        }

        async public Task UpdateMetadataProviders()
        {
            _providers = _providers ?? new ConcurrentBag<IMetadataProvider>();

            var metadataProviders = new PlugInManager().GetPluginInstances(typeof(IMetadataProvider)).ToArray();
            foreach (var metadataProvider in metadataProviders.Where(p => p is IMetadataProvider))
            {
                if (await ((IMetadataProvider)metadataProvider).ApplyTo(this))
                {
                    if (_providers.Where(p => p.GetType().Equals(metadataProvider.GetType())).Count() == 0)
                    {
                        _providers.Add((IMetadataProvider)metadataProvider);
                    }
                }
            }
        }

        async virtual public Task WriteMetadata(IPersistStream stream)
        {
            PlugInManager plugins = new PlugInManager();

            if (_providers != null)
            {
                foreach (IMetadataProvider provider in _providers)
                {
                    if (provider == null)
                    {
                        continue;
                    }

                    // mit ApplyTo noch einmal das this Objekt auf den Provider 
                    // setzen, damit beim speichern immer das aktuelle Object gesetzt wird...
                    await provider.ApplyTo(this);
                    stream.Save("IMetadataProvider", provider);
                }
            }
            else
            {
                _providers = new ConcurrentBag<IMetadataProvider>();
            }

            foreach (Type providerType in plugins.GetPlugins(typeof(IMetadataProvider)))
            {
                IMetadataProvider provider = plugins.CreateInstance(providerType) as IMetadataProvider;
                if (provider == null)
                {
                    continue;
                }

                // nach bereits vorhanden suchen...
                IMetadataProvider provider2 = this.MetadataProvider(PlugInManager.PlugInID(provider));
                if (provider2 != null)
                {
                    continue;
                }

                if (await provider.ApplyTo(this))
                {
                    stream.Save("IMetadataProvider", provider);
                    _providers.Add(provider);
                }
            }
        }

        public IMetadataProvider MetadataProvider(Guid guid)
        {
            if (_providers == null)
            {
                return null;
            }

            foreach (IMetadataProvider provider in _providers)
            {
                if (PlugInManager.PlugInID(provider).Equals(guid))
                {
                    return provider;
                }
            }
            return null;
        }

        public Task<IEnumerable<IMetadataProvider>> GetMetadataProviders()
        {
            if (_providers == null)
            {
                return Task.FromResult<IEnumerable<IMetadataProvider>>(new IMetadataProvider[0]);
            }

            return Task.FromResult<IEnumerable<IMetadataProvider>>(new ConcurrentBag<IMetadataProvider>(_providers));
        }

        async public Task SetMetadataProviders(IEnumerable<IMetadataProvider> providers, object Object = null, bool append = false)
        {
            Object = Object ?? this;
            if (append == true)
            {
                _providers = _providers ?? new ConcurrentBag<IMetadataProvider>();

                if (providers != null)
                {
                    foreach (IMetadataProvider provider in providers)
                    {
                        if (provider != null && await provider.ApplyTo(Object) &&
                            _providers.Where(p => p.GetType().Equals(provider.GetType())).Count() == 0)
                        {
                            _providers.Add(provider);
                        }
                    }
                }
            }
            else
            {
                _providers = new ConcurrentBag<IMetadataProvider>(); //ListOperations<IMetadataProvider>.Clone(value);

                if (providers != null)
                {
                    foreach (IMetadataProvider provider in providers)
                    {
                        if (provider != null && await provider.ApplyTo(Object))
                        {
                            _providers.Add(provider);
                        }
                    }
                }
            }
        }

        #endregion
    }

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
