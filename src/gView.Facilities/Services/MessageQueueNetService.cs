using gView.Facilities.Abstraction;
using MessageQueueNET.Client.Services;
using Microsoft.Extensions.Logging;

namespace gView.Facilities.Services;
internal class MessageQueueNetService : IMessageQueueService
{

    static internal string MessageQueueName = $"{Const.MessageQueuePrefix}{Guid.NewGuid().ToString("N").ToLower()}";

    private readonly MessageQueueAppTopicService _appTopicService;
    private readonly ILogger<MessageQueueNetService> _logger;

    public MessageQueueNetService(
            MessageQueueAppTopicService appTopicService,
            ILogger<MessageQueueNetService> logger
        )
    {
        _appTopicService = appTopicService;
        _logger = logger;
    }

    #region IMessageQueueService

    public Task<bool> RegisterQueueAsync(int lifetime = 0, int itemLifetime = 0)
        => _appTopicService.RegisterQueueAsync(lifetime, itemLifetime);


    async public Task<bool> EnqueueAsync(string queuePrefix, IEnumerable<string> messages, bool includeOwnQueue = false)
    {
        try
        {
            return await _appTopicService.EnqueueAsync(messages, includeOwnQueue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("An exception is thrown {exceptionMessage}", ex.Message);
            return false;
        }
    }

    #endregion
}
