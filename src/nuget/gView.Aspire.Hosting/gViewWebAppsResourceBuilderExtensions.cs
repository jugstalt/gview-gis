using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

static public class gViewWebAppsResourceBuilderExtensions
{
    private const string ContainerImage = "gview-webapps";
    private const string ContainerRegistry = "";
    private const string ContainerTag = "latest";

    public static gViewWebAppsResourceBuilder AddgViewWebApps(
            this IDistributedApplicationBuilder builder,
            string containerName,
            int? httpPort = null,
            int? httpsPort = null,
            string? imageTag = null
        )
    {
        var resource = new gViewWebAppsResource(containerName);

        var resourceBuilder = builder
                      .AddResource(resource)
                      .WithImage(ContainerImage)
                      .WithImageRegistry(ContainerRegistry)
                      .WithImageTag(imageTag ?? ContainerTag)
                      .WithVolume("gview-gis","/home/data")
                      .WithHttpEndpoint(
                          targetPort: 8080,
                          port: httpPort,
                          name: gViewServerResource.HttpEndpointName)
                      //.WithHttpsEndpoint(
                      //    targetPort: 8443,
                      //    port: httpsPort,
                      //    name: gViewServerResource.HttpsEndpointName);
                      .WithEnvironment(e =>
                      {
                          e.EnvironmentVariables.Add("RepositoryPath", "/home/data/gview-web-repository");
                      });

        return new gViewWebAppsResourceBuilder(
            builder,
            resourceBuilder);


    }

    public static gViewWebAppsResourceBuilder WithgViewServer(
            this gViewWebAppsResourceBuilder builder,
            IResourceBuilder<gViewServerResource> resourceBuilder,
            string title = "Apire gView Server",
            string description = "Aspire gView Server Instance for developing"
        )
    {
        builder.ResourceBuilder.WithEnvironment(e =>
        {
            e.EnvironmentVariables.Add($"CustomTiles__{builder.CustomTileIndex}__Title", title);
            e.EnvironmentVariables.Add($"CustomTiles__{builder.CustomTileIndex}__Description", description);
            e.EnvironmentVariables.Add($"CustomTiles__{builder.CustomTileIndex}__TargetUrl", resourceBuilder.Resource.HttpEndpoint.Url);

            e.EnvironmentVariables.Add($"Publish__Servers__{builder.CustomTileIndex}__Name", title);
            e.EnvironmentVariables.Add($"Publish__Servers__{builder.CustomTileIndex}__Url", resourceBuilder.Resource.HttpEndpoint.Url);
            e.EnvironmentVariables.Add($"Publish__Servers__{builder.CustomTileIndex}__Client", "carto-publish");
            e.EnvironmentVariables.Add($"Publish__Servers__{builder.CustomTileIndex}__Secret", "carto-publish-secret");

            builder.CustomTileIndex++;
        });

        return builder;
    }

    public static gViewWebAppsResourceBuilder WithDrive(
            this gViewWebAppsResourceBuilder builder,
            string name,
            string target,
            string mapToLocalDrive = ""
        )
    {
        builder.ResourceBuilder.WithEnvironment(e =>
        {
            e.EnvironmentVariables.Add($"Drives__{name.Replace(" ", "")}", target);
        });

        if(!string.IsNullOrEmpty(mapToLocalDrive))
        {
            builder.ResourceBuilder.WithBindMount(mapToLocalDrive, target); 
        }

        return builder;
    }

    public static IResourceBuilder<gViewWebAppsResource> Build(
       this gViewWebAppsResourceBuilder builder) => builder.ResourceBuilder;
}
