using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace gView.Server.Middleware
{
    public class ArcMapPathDoubleSlashesMiddleware
    {
        private readonly RequestDelegate _next;

        public ArcMapPathDoubleSlashesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            if (path.Contains("//"))
            {
                while (path.Contains("//"))
                {
                    path = path.Replace("//", "/");
                }

                context.Request.Path = new PathString(path);
            }

            return _next(context);
        }
    }
}
