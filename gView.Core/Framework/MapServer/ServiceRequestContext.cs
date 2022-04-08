using gView.Core.Framework.Exceptions;
using gView.Framework.Carto;
using gView.Framework.IO;
using System;
using System.Threading.Tasks;

namespace gView.MapServer
{
    public class ServiceRequestContext : IServiceRequestContext
    {
        private IMapServer _mapServer = null;
        private IServiceRequestInterpreter _interpreter = null;
        private ServiceRequest _request = null;

        private ServiceRequestContext(IMapServer mapServer, IServiceRequestInterpreter interpreter, ServiceRequest request)
        {
            _mapServer = mapServer;
            _interpreter = interpreter;
            _request = request;
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

        async public Task<IServiceMap> CreateServiceMapInstance()
        {
            var serviceMap = await _mapServer?.GetServiceMapAsync(this);
            if (serviceMap == null)
            {
                throw new MapServerException($"Unable to load service { _request.Folder }/{ _request.Service }: Check error log for details");
            }

            return serviceMap;
        }

        async public Task<IMetadataProvider> GetMetadtaProviderAsync(Guid metadataProviderId)
        {
            return await _mapServer?.GetMetadtaProviderAsync(this, metadataProviderId);
        }

        #endregion
    }
}
