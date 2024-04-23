using gView.Facilities.Abstraction;
using gView.Facilities.Services;
using MessageQueueNET.Client.Extensions;
using MessageQueueNET.Client.Extensions.DependencyInjetion;
using MessageQueueNET.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gView.Facilities.Extensions.DependencyInjection;
static public class ServicesExtensions
{
    static public IServiceCollection AddServerFacilities(
                    this IServiceCollection services,
                    IConfiguration configuration)
           => services
                .AddMessageQueueService(configuration)
                /*.AddHostedService<MessageQueueBackgroundWorker>()*/;

    //static private IServiceCollection AddMessageQueueService_(
    //                this IServiceCollection services,
    //                IConfiguration configuration
    //        )
    //    => configuration["Facilities:MessageQueue:Type"]?.ToLower() switch
    //    {
    //        null or "" => services.AddDummyMessageQueueService(),
    //        "messagequeue-net" => services.AddMessageQueueNetService(config =>
    //        {
    //            config.QueueServiceUrl = configuration["Facilities:MessageQueue:ConnectionString"] ?? "";
    //            config.ApiClient = configuration["Facilities:MessageQueue:Client"] ?? "";
    //            config.ApiClientSecret = configuration["Facilities:MessageQueue:ClientSecret"] ?? "";
    //            config.Namespace = configuration["Facilities:MessageQueue:Namespace"] ?? "";
    //            config.QueueName = MessageQueueNetService.MessageQueueName;
    //        }),
    //        _ => services
    //    };
    
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
        else if(queueType == "messagequeue-net")
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

                    config.LifetimeSeconds = 120;
                    config.TryRegisterQueues = true;
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
