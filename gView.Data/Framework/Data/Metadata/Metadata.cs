using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Framework.Data.Metadata
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
}
