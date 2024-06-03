using gView.Facilities.Abstraction;

namespace gView.Facilities.Services;
internal class DummyQueueService : IMessageQueueService
{
    public Task<bool> EnqueueAsync(IEnumerable<string> messages)
    {
        return Task.FromResult(true);
    }

    public Task<bool> EnqueueAsync(string queuePrefix, IEnumerable<string> messages, bool includeOwnQueue = true)
    {
        return Task.FromResult(true);
    }

    public Task<bool> RegisterQueueAsync(int lifetime = 0, int itemLifetime = 0)
    {
        return Task.FromResult(true);
    }
}
