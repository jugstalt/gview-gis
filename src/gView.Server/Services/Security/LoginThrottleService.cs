using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.Services.Security;

/// <summary>
/// Slows down password guessing: counts failed logins per user and per client IP
/// (in memory, per server instance). A user is locked after <c>max-failures</c>
/// failures within <c>window-minutes</c>, an IP after <c>max-failures-per-ip</c>;
/// both for <c>lockout-minutes</c>. While locked every login fails as if the
/// password were wrong.
/// Configuration (mapserver.json): <c>security:login:{max-failures, max-failures-per-ip, window-minutes, lockout-minutes}</c>.
/// </summary>
public class LoginThrottleService
{
    private const int MaxTrackedKeys = 50_000;

    private readonly ConcurrentDictionary<string, FailureCounter> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, FailureCounter> _ips = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger _logger;

    public LoginThrottleService(IConfiguration configuration, ILoggerFactory loggerFactory)
        : this(
            Int(configuration, "security:login:max-failures", 10),
            Int(configuration, "security:login:max-failures-per-ip", 30),
            TimeSpan.FromMinutes(Int(configuration, "security:login:window-minutes", 5)),
            TimeSpan.FromMinutes(Int(configuration, "security:login:lockout-minutes", 15)),
            loggerFactory?.CreateLogger(SecurityLog.Category))
    {
    }

    public LoginThrottleService(int maxFailures, int maxFailuresPerIp, TimeSpan window, TimeSpan lockout,
                                ILogger logger = null)
    {
        MaxFailures = maxFailures;
        MaxFailuresPerIp = maxFailuresPerIp;
        Window = window;
        Lockout = lockout;
        _logger = logger;
    }

    public int MaxFailures { get; }
    public int MaxFailuresPerIp { get; }
    public TimeSpan Window { get; }
    public TimeSpan Lockout { get; }

    /// <summary>For tests: the clock used for all decisions.</summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    public bool IsBlocked(string ip, string username)
    {
        var now = UtcNow();

        return IsLocked(_users, UserKey(username), now)
            || IsLocked(_ips, IpKey(ip), now);
    }

    public void Failed(string ip, string username)
    {
        var now = UtcNow();

        if (Register(_users, UserKey(username), MaxFailures, now))
        {
            _logger?.LogWarning("Login locked for user {username} for {minutes} minutes after {failures} failed attempts (last from {ip})",
                username, (int)Lockout.TotalMinutes, MaxFailures, ip);
        }
        if (Register(_ips, IpKey(ip), MaxFailuresPerIp, now))
        {
            _logger?.LogWarning("Logins locked for client {ip} for {minutes} minutes after {failures} failed attempts",
                ip, (int)Lockout.TotalMinutes, MaxFailuresPerIp);
        }
    }

    public void Succeeded(string username)
    {
        _users.TryRemove(UserKey(username), out _);
    }

    #region Helper

    private static string UserKey(string username) => (username ?? String.Empty).Trim().ToLowerInvariant();
    private static string IpKey(string ip) => String.IsNullOrWhiteSpace(ip) ? "unknown" : ip;

    private static bool IsLocked(ConcurrentDictionary<string, FailureCounter> counters, string key, DateTime now)
        => counters.TryGetValue(key, out var counter) && counter.IsLocked(now);

    /// <returns>true, when this failure locked the key</returns>
    private bool Register(ConcurrentDictionary<string, FailureCounter> counters, string key, int maxFailures, DateTime now)
    {
        if (maxFailures <= 0)
        {
            return false;
        }

        if (counters.Count >= MaxTrackedKeys)
        {
            Prune(counters, now);
        }

        return counters.GetOrAdd(key, _ => new FailureCounter()).Fail(now, Window, maxFailures, Lockout);
    }

    private void Prune(ConcurrentDictionary<string, FailureCounter> counters, DateTime now)
    {
        foreach (var key in counters.Where(c => c.Value.IsStale(now, Window)).Select(c => c.Key).ToArray())
        {
            counters.TryRemove(key, out _);
        }
    }

    private static int Int(IConfiguration configuration, string key, int defaultValue)
        => int.TryParse(configuration?[key], out int value) ? value : defaultValue;

    private class FailureCounter
    {
        private readonly Queue<DateTime> _failures = new();
        private DateTime _lockedUntil = DateTime.MinValue;

        public bool IsLocked(DateTime now)
        {
            lock (_failures)
            {
                return _lockedUntil > now;
            }
        }

        public bool IsStale(DateTime now, TimeSpan window)
        {
            lock (_failures)
            {
                return _lockedUntil <= now && (_failures.Count == 0 || _failures.Last() < now - window);
            }
        }

        public bool Fail(DateTime now, TimeSpan window, int maxFailures, TimeSpan lockout)
        {
            lock (_failures)
            {
                if (_lockedUntil > now)
                {
                    return false;
                }

                while (_failures.Count > 0 && _failures.Peek() < now - window)
                {
                    _failures.Dequeue();
                }

                _failures.Enqueue(now);

                if (_failures.Count >= maxFailures)
                {
                    _failures.Clear();
                    _lockedUntil = now + lockout;
                    return true;
                }

                return false;
            }
        }
    }

    #endregion
}
