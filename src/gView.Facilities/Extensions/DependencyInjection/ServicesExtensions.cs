using gView.Facilities.Abstraction;
using gView.Facilities.Services;
using MessageQueueNET.Client.Extensions.DependencyInjetion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Facilities.Extensions.DependencyInjection;
static public class ServicesExtensions
{
    static public IServiceCollection AddServerFacilities(
                    this IServiceCollection services,
                    IConfiguration configuration)
           => services
                .AddMessageQueueService(configuration);

    static private IServiceCollection AddMessageQueueService(
                this IServiceCollection services,
                IConfiguration configuration
        )
    {
        var queueType = configuration["Facilities:MessageQueue:Type"]?.ToLowerInvariant();

        if (String.IsNullOrEmpty(queueType))
        {
            services.AddDummyMessageQueueService();
        }
        else if (queueType == "messagequeue-net")
        {
            services
                .AddMessageQueueClientService()
                .AddMessageQueueAppTopicServices(config =>
                {
                    config.MessageQueueApiUrl = configuration["Facilities:MessageQueue:ConnectionString"] ?? "";
                    config.MessageQueueClientId = configuration["Facilities:MessageQueue:Client"] ?? "";
                    config.MessageQueueClientSecret = configuration["Facilities:MessageQueue:ClientSecret"] ?? "";

                    config.AppName = Const.MessageQueuePrefix;
                    config.Namespace = configuration["Facilities:MessageQueue:Namespace"] ?? "";

                    config.QueueLifetimeSeconds = 120;
                    config.ItemLifetimeSeconds = 120;
                    config.ManageQueueLiftimeCycle = true;
                })
                .AddMessageHandler<MessageQueueNetMessageHandler>();

            services.AddMessageHandler<MessageQueueNetMessageHandler>();

            services.AddMessageQueueNetService();
        }

        return services;
    }

    static private IServiceCollection AddDummyMessageQueueService(this IServiceCollection services)
        => services.AddTransient<IMessageQueueService, DummyQueueService>();

    static private IServiceCollection AddMessageQueueNetService(this IServiceCollection services)
        => services.AddTransient<IMessageQueueService, MessageQueueNetService>();

}
