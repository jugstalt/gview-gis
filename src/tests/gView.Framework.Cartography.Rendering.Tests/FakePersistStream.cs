using gView.Framework.Core.IO;

namespace gView.Framework.Cartography.Rendering.Tests;

/// <summary>
/// A minimal, in-memory <see cref="IPersistStream"/> test double - just enough of the real
/// contract (key/value get-or-default, no actual XML/serialization) to exercise
/// <c>SimpleLabelRenderer</c>/<c>AdvancedLabelRenderer</c>'s <c>Save</c>/<c>Load</c> round-trip and
/// backward-compatibility (a key that was never saved, e.g. simulating an older saved renderer
/// predating a newer property) without needing the real XML persistence machinery.
/// </summary>
internal sealed class FakePersistStream : IPersistStream
{
    private readonly Dictionary<string, object?> _values = new();

    public object? Load(string key) => _values.TryGetValue(key, out var v) ? v : null;
    public object? Load(string key, object? defVal) => _values.TryGetValue(key, out var v) ? v : defVal;
    public object? Load(string key, object? defVal, object? objectInstance) => _values.TryGetValue(key, out var v) ? v : defVal;

    public Task<T> LoadAsync<T>(string key, T objectInstance, T? defaultValue = default) where T : IPersistableLoadAsync
        => Task.FromResult(objectInstance);

    public Task<T> LoadPluginAsync<T>(string key, T? unknownPlugin = default) where T : IPersistableLoadAsync
        => Task.FromResult(unknownPlugin!);

    public void Save(string key, object? val) => _values[key] = val;
    public void SaveEncrypted(string key, string val) => _values[key] = val;

    public void RemoveKey(string key) => _values.Remove(key);

    public void AddWarning(string warning, object source) { }
    public void AddError(string error, object source) { }
    public IEnumerable<string> Warnings => Array.Empty<string>();
    public IEnumerable<string> Errors => Array.Empty<string>();
    public void ClearErrorsAndWarnings() { }
}
