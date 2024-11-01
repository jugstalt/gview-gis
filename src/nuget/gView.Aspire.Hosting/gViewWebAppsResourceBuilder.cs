using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public class gViewWebAppsResourceBuilder(
        IDistributedApplicationBuilder appBuilder,
        IResourceBuilder<gViewWebAppsResource> resourceBuilder
    )
{
    internal IDistributedApplicationBuilder AppBuilder { get; } = appBuilder;
    internal IResourceBuilder<gViewWebAppsResource> ResourceBuilder { get; } = resourceBuilder;
}
