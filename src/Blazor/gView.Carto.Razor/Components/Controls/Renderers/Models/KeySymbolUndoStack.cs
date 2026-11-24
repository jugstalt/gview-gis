using gView.Framework.Core.Symbology;

namespace gView.Carto.Razor.Components.Controls.Renderers.Models;

/// <summary>
/// Undo points of a class list: each point is a deep copy (cloned symbols) of the classes
/// and their feature counts before a change. The oldest points are dropped beyond <see cref="MaxDepth"/>.
/// </summary>
internal class KeySymbolUndoStack<TKey>
    where TKey : notnull
{
    public const int MaxDepth = 20;

    internal record Entry(string Action, List<KeySymbol<TKey>> KeySymbols, Dictionary<TKey, long>? Counts);

    private readonly LinkedList<Entry> _entries = new();

    public bool CanUndo => _entries.Count > 0;
    public int Count => _entries.Count;

    /// <summary>Name of the change the next undo takes back, e.g. "Recolor".</summary>
    public string? NextAction => _entries.Last?.Value.Action;

    public void Push(string action, IEnumerable<KeySymbol<TKey>> keySymbols, IReadOnlyDictionary<TKey, long>? counts)
    {
        var copy = keySymbols
            .Select(k => new KeySymbol<TKey>(k.Key, (ISymbol)k.Symbol.Clone()))
            .ToList();

        _entries.AddLast(new Entry(action, copy, counts?.ToDictionary(c => c.Key, c => c.Value)));

        while (_entries.Count > MaxDepth)
        {
            Release(_entries.First!.Value);
            _entries.RemoveFirst();
        }
    }

    /// <summary>
    /// The newest undo point. Its symbols now belong to the caller (they are not released here).
    /// </summary>
    public Entry? Pop()
    {
        var last = _entries.Last?.Value;
        if (last is not null)
        {
            _entries.RemoveLast();
        }

        return last;
    }

    public void Clear()
    {
        foreach (var entry in _entries)
        {
            Release(entry);
        }
        _entries.Clear();
    }

    private static void Release(Entry entry)
        => entry.KeySymbols.ForEach(k => k.Symbol?.Release());
}
