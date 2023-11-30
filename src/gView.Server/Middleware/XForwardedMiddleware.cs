using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace gView.Server.Middleware
{
    public class XForwardedMiddleware
    {
        private readonly RequestDelegate _next;

        public XForwardedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var xproto = context.Request.Headers["X-Forwarded-Proto"].ToString();
            if (xproto != null && xproto.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.Scheme = "https";
            }

            await _next(context);
        }
    }
}
