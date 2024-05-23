using gView.Server.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace gView.Server.Services.Security
{
    public class AccessTokenAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AccessTokenAuthServiceOptions _options;

        public AccessTokenAuthService(IHttpContextAccessor httpContextAccessor,
                                      IOptionsMonitor<AccessTokenAuthServiceOptions> optionsMonitor)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = optionsMonitor.CurrentValue;
        }

        async public Task<string> GetAccessTokenUsername()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;

                if (_options.AllowAccessTokenAuthorization)
                {
                    var accessToken = context.Request.Query[_options.AccessTokenParameterName].ToString();

                    if (!String.IsNullOrEmpty(accessToken))
                    {
                        var securityToken = await accessToken.ToValidatedJwtSecurityToken(_options.Authority);

                        if (securityToken.ValidTo < DateTime.UtcNow)
                        {
                            throw new Exception("Token expired");
                        }

                        if (!String.IsNullOrEmpty(securityToken.Subject))
                        {
                            return securityToken.Subject;
                        }
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
