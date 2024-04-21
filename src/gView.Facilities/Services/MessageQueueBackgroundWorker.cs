using gView.Facilities.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gView.Facilities.Services;
internal class MessageQueueBackgroundWorker : BackgroundService
{
    private readonly IMessageQueueService? _messageQueueService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageQueueBackgroundWorker> _logger;

    public MessageQueueBackgroundWorker(
                    ILogger<MessageQueueBackgroundWorker> logger,
                    IServiceProvider serviceProvider,
                    IMessageQueueService? messageQueueService = null
            )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _messageQueueService = messageQueueService; 
    }

    async protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_messageQueueService is null)
        {
            _logger.LogWarning("No IMessageQueueService initialized. Quit MessageQueueBackgroundWorker");
            return;
        }

        // register
        await _messageQueueService.RegisterQueueAsync(_messageQueueService.RecommendedWorkerDelayMilliseconds * 30 / 1000);
        
        _logger.LogInformation("Listening to {messageQueueType} for instance messages", _messageQueueService.GetType().Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWork(); 
            await Task.Delay(
                    Math.Max(100, _messageQueueService.RecommendedWorkerDelayMilliseconds),
                    stoppingToken
                );
        }
    }

    async private Task DoWork()
    {
        if (_messageQueueService is null) return;

        var messages = await _messageQueueService!.DequeueAsync(10);

        if (messages is null) return;

        foreach (var message in messages)
        {
            if (message.Contains(":"))
            {
                string messageKey = message.Split(':')[0];
                string messageContent = message.Substring(messageKey.Length + 1);

                var handler = _serviceProvider.GetKeyedService<IMessageHandler>(messageKey);

                await (handler?.InvokeAsync(messageContent) ?? Task.CompletedTask);
            }
        }
    }
}
