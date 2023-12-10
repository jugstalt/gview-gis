using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.MapServer
{
    public class ServiceRequestContext : IServiceRequestContext
    {
        private readonly IMapServer _mapServer = null;
        private readonly IServiceRequestInterpreter _interpreter = null;
        private readonly ServiceRequest _request = null;

        private ServiceRequestContext(IMapServer mapServer, IServiceRequestInterpreter interpreter, ServiceRequest request)
        {
            _mapServer = mapServer;
            _interpreter = interpreter;
            _request = request;

            if (ContextVariables.UseMetrics)
            {
                Metrics = new ConcurrentDictionary<string, double>();
            }
        }

        async static public Task<IServiceRequestContext> TryCreate(IMapServer mapServer, IServiceRequestInterpreter interpreter, ServiceRequest request, bool checkSecurity = true)
        {
            var context = new ServiceRequestContext(mapServer, interpreter, request);

            if (checkSecurity == true)
            {
                var mapService = mapServer?.GetMapService(request?.Service, request?.Folder);
                if (mapService == null)
                {
                    throw new MapServerException("Unknown service");
                }

                await mapService.CheckAccess(context);
            }

            return context;
        }

        #region IServiceRequestContext Member

        public IMapServer MapServer
        {
            get { return _mapServer; }
        }

        public IServiceRequestInterpreter ServiceRequestInterpreter
        {
            get { return _interpreter; }
        }

        public ServiceRequest ServiceRequest
        {
            get { return _request; }
        }

        public IDictionary<string, double> Metrics { get; }

        async public Task<IServiceMap> CreateServiceMapInstance()
        {
            var serviceMap = await _mapServer?.GetServiceMapAsync(this);
            if (serviceMap == null)
            {
                throw new MapServerException($"Unable to load service {_request.Folder}/{_request.Service}: Check error log for details");
            }

            return serviceMap;
        }

        async public Task<IMetadataProvider> GetMetadtaProviderAsync(Guid metadataProviderId)
        {
            return await _mapServer?.GetMetadtaProviderAsync(this, metadataProviderId);
        }

        #region Context Metadata

        private ConcurrentDictionary<string, object> _metadata;
        public void SetContextMetadata<T>(string key, T value)
        {
            if (_metadata == null)
            {
                _metadata = new ConcurrentDictionary<string, object>();
            }

            _metadata[key] = value;
        }

        public T GetContextMetadata<T>(string key, T defaultValue = default)
        {
            if (_metadata?.TryGetValue(key, out var value) == true)
            {
                return (T)value;
            }

            return defaultValue;
        }

        #endregion

        #endregion
    }
}
