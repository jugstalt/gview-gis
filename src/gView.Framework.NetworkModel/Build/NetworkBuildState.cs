namespace gView.Framework.NetworkModel.Build;

/// <summary>The state and log of a network build started from Carto (kept by the <see cref="Module.NetworkModule"/>).</summary>
public sealed class NetworkBuildState
{
    private readonly List<string> _log = [];

    public DateTime Started { get; } = DateTime.Now;
    public DateTime? Finished { get; internal set; }
    public bool Running => Finished is null;
    public bool? Succeeded { get; internal set; }
    public string? Error { get; internal set; }

    /// <summary>Raised from the build thread whenever a line was added or the build ended.</summary>
    public event Action? Changed;

    public IReadOnlyList<string> Log
    {
        get { lock (_log) { return _log.ToList(); } }
    }

    internal void AddLine(string line)
    {
        lock (_log) { _log.Add($"{DateTime.Now:HH:mm:ss} {line}"); }
        Changed?.Invoke();
    }

    internal void Notify() => Changed?.Invoke();
}

/// <summary>What is known about the built network of a map.</summary>
public sealed record NetworkStatus(string? Directory, bool Built, DateTime? Created, int NodeCount, int EdgeCount, int ShardCount, bool Stale, string? StaleReason)
{
    public static NetworkStatus NotBuilt(string? directory) => new(directory, false, null, 0, 0, 0, false, null);
}
