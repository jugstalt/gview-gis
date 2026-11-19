using gView.Server.Services.Security;

namespace gView.Server.Tests;

public class LoginThrottleServiceTests
{
    private DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private LoginThrottleService Create(int maxFailures = 3, int maxFailuresPerIp = 10)
        => new(maxFailures, maxFailuresPerIp, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15))
        {
            UtcNow = () => _now
        };

    [Fact]
    public void Failed_BelowLimit_DoesNotBlock()
    {
        var throttle = Create();

        throttle.Failed("1.1.1.1", "admin");
        throttle.Failed("1.1.1.1", "admin");

        Assert.False(throttle.IsBlocked("1.1.1.1", "admin"));
    }

    [Fact]
    public void Failed_ReachingLimit_BlocksUserFromAnyIp()
    {
        var throttle = Create();

        for (int i = 0; i < 3; i++)
        {
            throttle.Failed($"1.1.1.{i}", "admin");
        }

        Assert.True(throttle.IsBlocked("9.9.9.9", "ADMIN"));
        Assert.False(throttle.IsBlocked("9.9.9.9", "other"));
    }

    [Fact]
    public void Lockout_ExpiresAfterLockoutTime()
    {
        var throttle = Create();
        for (int i = 0; i < 3; i++)
        {
            throttle.Failed("1.1.1.1", "admin");
        }

        _now = _now.AddMinutes(16);

        Assert.False(throttle.IsBlocked("1.1.1.1", "admin"));
    }

    [Fact]
    public void Failures_OutsideWindow_AreForgotten()
    {
        var throttle = Create();

        throttle.Failed("1.1.1.1", "admin");
        throttle.Failed("1.1.1.1", "admin");
        _now = _now.AddMinutes(6);
        throttle.Failed("1.1.1.1", "admin");

        Assert.False(throttle.IsBlocked("1.1.1.1", "admin"));
    }

    [Fact]
    public void Succeeded_ResetsUserFailures()
    {
        var throttle = Create();

        throttle.Failed("1.1.1.1", "admin");
        throttle.Failed("1.1.1.1", "admin");
        throttle.Succeeded("admin");
        throttle.Failed("1.1.1.1", "admin");

        Assert.False(throttle.IsBlocked("1.1.1.1", "admin"));
    }

    [Fact]
    public void Failed_ManyUsersFromOneIp_BlocksIp()
    {
        var throttle = Create(maxFailures: 100, maxFailuresPerIp: 5);

        for (int i = 0; i < 5; i++)
        {
            throttle.Failed("6.6.6.6", $"user{i}");
        }

        Assert.True(throttle.IsBlocked("6.6.6.6", "someone"));
        Assert.False(throttle.IsBlocked("7.7.7.7", "someone"));
    }
}
