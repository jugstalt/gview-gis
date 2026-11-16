using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Store;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Types;

/// <summary>
/// Turns the texts of the facet templates into numbers and every combination into a type id.
/// Predefined values of the config win; the numbering of an earlier build is reused, so ids stay stable between builds.
/// An empty text is 0 ("not set"); type 0 is reserved for helper nodes.
/// </summary>
public sealed class TypeCatalog
{
    private sealed class FacetValues
    {
        public required string Name { get; init; }
        public Dictionary<string, int> ByText { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<int, string> ByNumber { get; } = [];
        public int Next => ByNumber.Count == 0 ? 1 : ByNumber.Keys.Max() + 1;

        public void Set(int number, string text)
        {
            if (number <= 0)
            {
                throw new InvalidDataException($"Facet '{Name}': values must be greater than 0.");
            }
            if (ByText.TryGetValue(text, out var existing) && existing != number)
            {
                ByNumber.Remove(existing);
            }
            if (ByNumber.TryGetValue(number, out var oldText))
            {
                ByText.Remove(oldText);
            }
            ByText[text] = number;
            ByNumber[number] = text;
        }
    }

    private readonly List<FacetValues> _facets;
    private readonly Dictionary<string, NetworkType> _typesByKey = [];
    private readonly Dictionary<int, NetworkType> _typesById = [];
    private readonly HashSet<int> _used = [];

    public TypeCatalog(IReadOnlyList<FacetConfig> facets, NetworkMetadata? previous)
    {
        _facets = facets.Select(f => new FacetValues { Name = f.Name }).ToList();

        // numbering of the earlier build first, then the config wins
        if (previous is not null)
        {
            foreach (var prevFacet in previous.Facets)
            {
                var facet = _facets.FirstOrDefault(f => f.Name.Equals(prevFacet.Name, StringComparison.OrdinalIgnoreCase));
                if (facet is null)
                {
                    continue;
                }
                foreach (var (number, text) in prevFacet.Values)
                {
                    facet.Set(number, text);
                }
            }
        }
        for (int i = 0; i < facets.Count; i++)
        {
            foreach (var (number, text) in facets[i].Values)
            {
                _facets[i].Set(number, text);
            }
        }

        if (previous is not null && previous.Facets.Count == _facets.Count
            && previous.Facets.Select(f => f.Name).SequenceEqual(_facets.Select(f => f.Name), StringComparer.OrdinalIgnoreCase))
        {
            foreach (var type in previous.Types.Where(t => t.Id != 0))
            {
                var networkType = new NetworkType(type.Id, type.Kind, type.Facets, type.Name);
                _typesByKey[Key(type.Kind, type.Facets)] = networkType;
                _typesById[type.Id] = networkType;
            }
        }
    }

    public int FacetCount => _facets.Count;

    /// <summary>The type id of an element with these facet texts (one per facet).</summary>
    public int Resolve(ElementKind kind, IReadOnlyList<string> texts)
    {
        var numbers = new int[_facets.Count];
        for (int i = 0; i < numbers.Length; i++)
        {
            var text = texts[i]?.Trim() ?? "";
            if (text.Length == 0)
            {
                continue;
            }
            var facet = _facets[i];
            if (!facet.ByText.TryGetValue(text, out numbers[i]))
            {
                numbers[i] = facet.Next;
                facet.Set(numbers[i], text);
            }
        }

        var key = Key(kind, numbers);
        if (!_typesByKey.TryGetValue(key, out var type))
        {
            int id = _typesById.Count == 0 ? 1 : _typesById.Keys.Max() + 1;
            var name = string.Join('.', numbers.Select((n, i) => n == 0 ? null : _facets[i].ByNumber[n]).Where(t => t is not null));
            type = new NetworkType(id, kind, numbers, name);
            _typesByKey[key] = type;
            _typesById[id] = type;
        }
        _used.Add(type.Id);
        return type.Id;
    }

    /// <summary>The registry for SpatialNET: every facet value and every type known so far.</summary>
    public TypeRegistry ToRegistry()
        => new(
            _facets.Select(f => new Facet(f.Name, new Dictionary<int, string>(f.ByNumber))).ToList(),
            _typesById.Values.OrderBy(t => t.Id));

    public IEnumerable<NetworkFacetMetadata> FacetMetadata()
        => _facets.Select(f => new NetworkFacetMetadata
        {
            Name = f.Name,
            Values = f.ByNumber.OrderBy(v => v.Key).ToDictionary(v => v.Key, v => v.Value)
        });

    public IEnumerable<NetworkTypeMetadata> TypeMetadata()
        => _typesById.Values.OrderBy(t => t.Id).Select(t => new NetworkTypeMetadata
        {
            Id = t.Id,
            Kind = t.Kind,
            Facets = t.Facets.ToArray(),
            Name = t.Name,
            Used = _used.Contains(t.Id)
        });

    private static string Key(ElementKind kind, IReadOnlyList<int> facets) => $"{(int)kind}:{string.Join(',', facets)}";
}
