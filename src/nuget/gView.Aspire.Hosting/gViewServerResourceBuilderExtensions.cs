using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

static public class gViewServerResourceBuilderExtensions
{
    private const string ContainerImage = "gview-server";
    private const string ContainerRegistry = "";
    private const string ContainerTag = "latest";

    public static gViewServerResourceBuilder AddgViewServer(
            this IDistributedApplicationBuilder builder,
            string containerName,
            int? httpPort = null,
            int? httpsPort = null,
            string? imageTag = null
        )
    {
        var resource = new gViewServerResource(containerName);

        var resourceBuilder = builder.AddResource(resource)
                      .WithImage(ContainerImage)
                      .WithImageRegistry(ContainerRegistry)
                      .WithImageTag(imageTag ?? ContainerTag)
                      .WithHttpEndpoint(
                          targetPort: 8080,
                          port: httpPort,
                          name: gViewServerResource.HttpEndpointName)
                      //.WithHttpsEndpoint(
                      //    targetPort: 8443,
                      //    port: httpsPort,
                      //    name: gViewServerResource.HttpsEndpointName);
                      .WithVolume("gview-gis", "/home/app")
                      .WithEnvironment(e =>
                      {
                          e.EnvironmentVariables.Add("ServicesFolder", "/home/app/gview-server-repository/server/configuraiton");
                          e.EnvironmentVariables.Add("OutputPath", "/home/app/gview-server-repository/server/web/output");
                          e.EnvironmentVariables.Add("OutputUrl", $"{resource.HttpEndpoint.Url}/output");
                          e.EnvironmentVariables.Add("OnlineResourceUrl", $"{resource.HttpEndpoint.Url}");
                          e.EnvironmentVariables.Add("TileCacheRoot", "/home/app/gview-server-repository/server/web/tile-caches");
                      });

        return new gViewServerResourceBuilder(
            builder,
            resourceBuilder);


    }

    public static IResourceBuilder<gViewServerResource> Build(
       this gViewServerResourceBuilder builder) => builder.ResourceBuilder;
}
