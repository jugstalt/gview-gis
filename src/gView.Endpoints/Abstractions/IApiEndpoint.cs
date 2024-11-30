using Microsoft.AspNetCore.Routing;

namespace gView.Endpoints.Abstractions;

public interface IApiEndpoint
{
    void Register(IEndpointRouteBuilder app);
}
