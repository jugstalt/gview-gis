using gView.Cmd.Services;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Cmd.Extensions.DependencyInjection;
internal static class ServicesCollectionExtensions
{
    static public IServiceCollection AddCommandCollection(this IServiceCollection services)
        => services.AddSingleton<CommandCollectionService>();

    static public IServiceCollection AddCommandLineArguments(this IServiceCollection services, Action<CommandLineArgumentsServiceOptions> setupAction)
        => services
            .Configure(setupAction)
            .AddSingleton<CommandLineArgumentsService>();

    static public IServiceCollection AddWorker(this IServiceCollection services)
        => services.AddSingleton<WorkerService>();
}
