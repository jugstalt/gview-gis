using gView.Framework.Core.Common;
using gView.Framework.Core.MapServer;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Core.Extensions;

public static class ServiceRequestContextErrorExtensions
{
    extension(IServiceRequestContext context)
    {
        /// <summary>
        /// Logs the full exception (with the error id of the request) to the service error log
        /// and returns the message for the client (see <see cref="ExceptionExtensions"/>).
        /// Client exceptions (MapServerException) are logged with their message only.
        /// </summary>
        async public Task<string> LogServiceException(Exception exception, string header)
        {
            string errorId = context?.ServiceRequest?.ErrorId;

            if (context?.MapServer is not null)
            {
                try
                {
                    await context.MapServer.LogAsync(context, header, loggingMethod.error,
                        exception.IsClientMessage
                            ? exception.Message
                            : exception.ToLogMessage(errorId));
                }
                catch { }
            }

            return exception.ToClientMessage(errorId);
        }
    }
}
