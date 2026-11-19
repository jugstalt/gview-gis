using gView.Framework.Common.Json;
using gView.Framework.Core.Extensions;
using gView.Framework.Core.MapServer;
using gView.Interoperability.GeoServices.Exceptions;
using gView.Interoperability.GeoServices.Rest.DTOs;
using gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer;
using System;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Extensions;

static internal class ServiceRequestContextExtensions
{
    extension(IServiceRequestContext context)
    {
        async public Task HandleMapServerException(Exception exception)
        {
            var (code, message) = await context.ToGeoServicesError(exception);

            context.ServiceRequest.Succeeded = false;
            context.ServiceRequest.Response = new JsonErrorDTO()
            {
                Error = new JsonErrorDTO.ErrorDef()
                {
                    Code = code,
                    Message = message
                }
            };
        }

        async public Task HandleFeatureServerException(Exception exception)
        {
            var (code, message) = await context.ToGeoServicesError(exception);

            context.ServiceRequest.Succeeded = false;
            context.ServiceRequest.Response = JSerializer.Serialize(new JsonFeatureServerResponseDTO()
            {
                AddResults = new JsonFeatureServerResponseDTO.JsonResponse[]
                {
                    new JsonFeatureServerResponseDTO.JsonResponse()
                    {
                        Success=false,
                        Error=new JsonFeatureServerResponseDTO.JsonError()
                        {
                            Code = code,
                            Description = message
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Error code and client message for an exception. GeoServicesException and
        /// MapServerException texts are meant for clients (400 or their own code); every
        /// other exception is logged with the error id and answered with 500 and a generic text.
        /// </summary>
        async public Task<(int code, string message)> ToGeoServicesError(Exception exception)
        {
            if (exception is GeoServicesException gse)
            {
                if (context.MapServer is not null)
                {
                    await context.MapServer.LogAsync(context, context.ServiceRequest.Method, Framework.Core.Common.loggingMethod.error, gse.Message);
                }
                return (gse.ErrorCode, gse.Message);
            }

            string message = await context.LogServiceException(exception, context.ServiceRequest.Method);
            return (exception.IsClientMessage ? 400 : 500, message);
        }
    }
}
