using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Server.Extensions;

internal static class HttpRequestExtensions
{
    async static public Task<string> GetBody(this HttpRequest httpRequest)
    {
        if (httpRequest.Body.CanRead)
        {
            using (var reader = new StreamReader(httpRequest.Body))
            {
                var body = await reader.ReadToEndAsync();
                return body;
            }
        }

        return String.Empty;
    }
}
