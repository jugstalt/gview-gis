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
                // ArcGIS Server
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
                   name: "arcgis_rest_servicelayer",
                   template: "arcgis/rest/services/{folder}/{id}/mapserver/{layerId}",
                   defaults: new { controller = "ArcGis", Action = "ServiceLayer" }
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

                // Ogc
                routes.MapRoute(
                    name: "ogc_request",
                    template: "ogc/{id}",
                    defaults: new { controller = "Ogc", Action = "OgcRequest" }
                );

                // ArcIMS
                routes.MapRoute(
                    name: "ags-servlet",
                    template: "servlet/com.esri.esrimap.Esrimap",
                    defaults: new { controller = "ArcIMS", Action = "Esrimap" }
                );
                routes.MapRoute(
                    name: "ags-servlet2",
                    template: "arcims/servlet/com.esri.esrimap.Esrimap",
                    defaults: new { controller = "ArcIMS", Action = "Esrimap" }
                );

                // MapServer
                routes.MapRoute(
                    name: "mapserver-catelog",
                    template: "catalog",
                    defaults: new { controller = "MapServer", Action = "Catalog" }
                );
                routes.MapRoute(
                    name: "mapserver-maprequest",
                    template: "MapRequest/{guid}/{id}",
                    defaults: new { controller = "MapServer", Action = "MapRequest" }
                );
                routes.MapRoute(
                    name: "mapserver-addmap",
                    template: "AddMap/{name}",
                    defaults: new { controller = "MapServer", Action = "AddMap" }
                );
                routes.MapRoute(
                    name: "mapserver-remotemap",
                    template: "RemoveMap/{name}",
                    defaults: new { controller = "MapServer", Action = "RemoveMap" }
                );
                routes.MapRoute(
                    name: "mapserver-getmetadata",
                    template: "GetMetadata/{name}",
                    defaults: new { controller = "MapServer", Action = "GetMetadata" }
                );
                routes.MapRoute(
                    name: "mapserver-setmetadata",
                    template: "SetMetadata/{name}",
                    defaults: new { controller = "MapServer", Action = "SetMetadata" }
                );

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            PlugInManager.Init();
            //InternetMapServer.Init(@"C:\ProgramData\gView\mapServer\Services\8001");
            InternetMapServer.Init(@"C:\Development_OpenSource\GeoDaten\MXL\8050");
        }
    }
}
