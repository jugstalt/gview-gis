using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

static public class gViewServerResourceBuilderExtensions
{
    private const string ContainerImage = "";
    private const string ContainerRegistry = "";
    private const string ContainerTag = "latest";

    public static gViewServerResourceBuilder AddIdentityServerNET(
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
                      .WithHttpsEndpoint(
                          targetPort: 8443,
                          port: httpsPort,
                          name: gViewServerResource.HttpsEndpointName);

        return new gViewServerResourceBuilder(
            builder,
            resourceBuilder);


    }

    public static IResourceBuilder<gViewServerResource> Build(
       this gViewServerResourceBuilder builder) => builder.ResourceBuilder;
}
