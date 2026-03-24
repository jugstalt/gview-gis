using gView.Framework.Common.Json;
using gView.Framework.Core.Exceptions;
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
            // ToDo: Log Exception

            await context.MapServer.LogAsync(
                context,
                context.ServiceRequest.Method,
                Framework.Core.Common.loggingMethod.error,
                exception is NullReferenceException
                            ? $"{exception.Message}\n{exception.StackTrace}"
                            : exception.Message
                );

            context.ServiceRequest.Succeeded = false;
            context.ServiceRequest.Response = new JsonErrorDTO()
            {
                Error = new JsonErrorDTO.ErrorDef()
                {
                    Code = exception is GeoServicesException
                        ? ((GeoServicesException)exception).ErrorCode
                        : 400,
                    Message = exception is MapServerException
                        ? exception.Message
                        : exception is NullReferenceException
                            ? $"{exception.Message}\n{exception.StackTrace}"
                            : "Bad Request"
                }
            };
        }

        async public Task HandleFeatureServerException(Exception exception)
        {
            // ToDo: Log Exception

            await context.MapServer.LogAsync(
                context,
                context.ServiceRequest.Method,
                Framework.Core.Common.loggingMethod.error,
                exception is NullReferenceException
                            ? $"{exception.Message}\n{exception.StackTrace}"
                            : exception.Message
                );

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
                            Code = exception is GeoServicesException
                                ? ((GeoServicesException)exception).ErrorCode
                                : 400,
                            Description = exception is MapServerException
                                ? exception.Message
                                : exception is NullReferenceException
                                    ? $"{exception.Message}\n{exception.StackTrace}"
                                    : "Bad Request"
                        }
                    }
                }
            });
        }
    }
}
