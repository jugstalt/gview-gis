using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

static public class gViewWebAppsResourceBuilderExtensions
{
    private const string ContainerImage = "";
    private const string ContainerRegistry = "";
    private const string ContainerTag = "latest";

    public static gViewWebAppsResourceBuilder AddIdentityServerNET(
            this IDistributedApplicationBuilder builder,
            string containerName,
            int? httpPort = null,
            int? httpsPort = null,
            string? imageTag = null
        )
    {
        var resource = new gViewWebAppsResource(containerName);

        var resourceBuilder = builder.AddResource(resource)
                      .WithImage(ContainerImage)
                      .WithImageRegistry(ContainerRegistry)
                      .WithImageTag(imageTag ?? ContainerTag)
                      .WithHttpEndpoint(
                          targetPort: 8080,
                          port: httpPort,
                          name: gViewServerResource.HttpEndpointName)
                      .WithHttpsEndpoint(
                          targetPort: 8443,
                          port: httpsPort,
                          name: gViewServerResource.HttpsEndpointName);

        return new gViewWebAppsResourceBuilder(
            builder,
            resourceBuilder);


    }

    public static IResourceBuilder<gViewWebAppsResource> Build(
       this gViewWebAppsResourceBuilder builder) => builder.ResourceBuilder;
}
