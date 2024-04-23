namespace gView.Facilities.Abstraction;
public interface IMessageQueueService
{
    Task<bool> RegisterQueueAsync(int lifetime = 0, int itemLifetime = 0);

    Task<bool> EnqueueAsync(string queuePrefix, IEnumerable<string> messages, bool includeOwnQueue = false);
}
