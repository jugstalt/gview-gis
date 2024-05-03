using gView.Blazor.Core.Services.Abstraction;

namespace gView.Web.Services;

public class WebScopeContextService : IScopeContextService
{
    private readonly Dictionary<string, string> _requestParameters = new Dictionary<string, string>();

    public WebScopeContextService(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if(httpContext?.Request?.Query.Keys is not null)
        {
            foreach(var key in httpContext.Request.Query.Keys)
            {
                _requestParameters.Add(key, httpContext.Request.Query[key].ToString());
            }
        }
    }

    public IDictionary<string, string> RequestParameters => _requestParameters;
}
