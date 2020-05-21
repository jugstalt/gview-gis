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
            Action<InternetMapServerServiceOptions> configAction,
            Action<InternetMapServerCapabilites> configCapabilites = null)
        {
            services.Configure<InternetMapServerServiceOptions>(configAction);
            services.AddSingleton<InternetMapServerService>();

            services.AddSingleton<UrlHelperService>();
            services.AddSingleton<LoginManagerService>();
            services.AddSingleton<AccessControlService>();
            services.AddSingleton<EncryptionCertificateService>();

            services.AddSingleton<MapServerDeployService>();
            services.AddSingleton<MapServicesEventLogger>();

            var capabilites = new InternetMapServerCapabilites();
            configCapabilites?.Invoke(capabilites);

            if(capabilites.AllowDeployment)
            {
                
            }

            return services;
        } 
    }
}
