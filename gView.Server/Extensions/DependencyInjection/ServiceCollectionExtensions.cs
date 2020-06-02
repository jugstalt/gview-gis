using gView.Server.Services.Hosting;
using gView.Server.Services.Logging;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Extensions.DependencyInjection
{
    static public class ServiceCollectionExtensions
    {
        static public IServiceCollection AddMapServerService(
            this IServiceCollection services,
            Action<MapServerManagerOptions> configAction)
        {
            services.Configure<MapServerManagerOptions>(configAction);
            services.AddSingleton<MapServiceManager>();

            services.AddSingleton<UrlHelperService>();
            services.AddSingleton<LoginManager>();
            services.AddSingleton<AccessControlService>();
            services.AddSingleton<EncryptionCertificateService>();

            services.AddSingleton<MapServiceDeploymentManager>();
            services.AddSingleton<MapServicesEventLogger>();

            return services;
        } 
    }
}
