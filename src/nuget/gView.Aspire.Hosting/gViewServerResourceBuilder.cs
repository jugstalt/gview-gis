using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public class gViewServerResourceBuilder(
        IDistributedApplicationBuilder appBuilder,
        IResourceBuilder<gViewServerResource> resourceBuilder
    )
{
    internal IDistributedApplicationBuilder AppBuilder { get; } = appBuilder;
    internal IResourceBuilder<gViewServerResource> ResourceBuilder { get; } = resourceBuilder;
}
