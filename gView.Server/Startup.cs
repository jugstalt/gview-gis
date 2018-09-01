using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gView.Framework.system;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "arcgis_rest_exportmap",
                    template: "arcgis/rest/services/{folder}/{id}/mapserver/export",
                    defaults: new { controller = "ArcGis", Action = "ExportMap" }
                );
                routes.MapRoute(
                    name: "arcgis_rest_servicelayers",
                    template: "arcgis/rest/services/{folder}/{id}/mapserver/layers",
                    defaults: new { controller = "ArcGis", Action = "ServiceLayers" }
                );
                routes.MapRoute(
                    name: "arcgis_rest_service",
                    template: "arcgis/rest/services/{folder}/{id}/mapserver",
                    defaults: new { controller = "ArcGis", Action = "Service" }
                );
                routes.MapRoute(
                    name: "arcgis_rest_folder",
                    template: "arcgis/rest/services/{id}",
                    defaults: new { controller = "ArcGis", Action="Folder" }
                );
                routes.MapRoute(
                    name: "arcgis_rest_services",
                    template: "arcgis/rest/services",
                    defaults: new { controller = "ArcGis", Action = "Services" }
                );
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            PlugInManager.Init();
            InternetMapServer.Init(@"C:\ProgramData\gView\mapServer\Services\8001");
            
            var maps = InternetMapServer.Instance.Maps;
            var map = InternetMapServer.Instance["Map1"];
        }
    }
}
