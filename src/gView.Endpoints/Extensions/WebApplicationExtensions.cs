using gView.Endpoints.Abstractions;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Endpoints.Extensions;

static public class WebApplicationExtensions
{
    static public IEndpointRouteBuilder RegisterApiEndpoints(
                this IEndpointRouteBuilder app,
                Type assemblyType)
    {
        var apiEndpointTypes = assemblyType.Assembly.GetTypes()
            .Where(
                t => typeof(IApiEndpoint).IsAssignableFrom(t)
                && t.GetConstructor([]) != null);

        Console.WriteLine("Register ApiEndpoints");
        Console.WriteLine("=====================");

        foreach (var apiEndpointType in apiEndpointTypes)
        {
            try
            {
                Console.Write($"Register ApiEndpoint {apiEndpointType}");

                var apiEndpoint = Activator.CreateInstance(apiEndpointType) as IApiEndpoint;

                apiEndpoint?.Register(app);

                Console.WriteLine("...succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"...failed: {ex.Message}");
            }
        }

        Console.WriteLine("...done");

        return app;
    }
}
