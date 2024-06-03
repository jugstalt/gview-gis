using gView.Facilities.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace gView.Facilities.Services;

[MessageQueueNET.Core.Reflection.MessageHandler(CommandName = CommandName)]
internal class MessageQueueNetMessageHandler : MessageQueueNET.Core.Services.Abstraction.IMessageHandler
{
    public const string CommandName = "MQNetHandler";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageQueueNetMessageHandler> _logger;

    public MessageQueueNetMessageHandler(
                IServiceProvider serviceProvider,
                ILogger<MessageQueueNetMessageHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    async public Task InvokeAsync(string message)
    {
        try
        {
            if (message.Contains(":"))
            {
                var messageKey = message.Split(':')[0];
                var messageValue = message.Substring(messageKey.Length + 1);

                var handler = _serviceProvider.GetKeyedService<IMessageHandler>(messageKey);

                if (handler is not null)
                {
                    await handler.InvokeAsync(messageValue);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Exception: {exceptionMessage}", ex.Message);
        }
    }
}
