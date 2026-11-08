using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.UI;
using System.Text;

namespace gView.Framework.Cartography.VectorTiles;

/// <summary>
/// The MVT layer names of a map: one per map layer (not per feature class), built from the
/// group path and the layer name, normalized to <c>[a-z0-9_]</c> and made unique with a suffix.
/// </summary>
/// <remarks>
/// The names are computed over all layers of the map (visible or not, any scale) in TOC order,
/// so a layer has the same name in every tile and in the style.
/// </remarks>
public sealed class MvtLayerNames
{
    private readonly Dictionary<ILayer, string> _names = new();

    public MvtLayerNames(IToc? toc, IEnumerable<IDatasetElement> mapElements)
    {
        var used = new HashSet<string>(StringComparer.Ordinal);

        foreach (var layer in mapElements.OfType<ILayer>())
        {
            var tocElement = toc?.GetTOCElement(layer);
            string raw = tocElement != null ? TocPath(tocElement) : layer.Title;
            string name = Normalize(raw);

            string unique = name;
            for (int i = 2; !used.Add(unique); i++)
            {
                unique = $"{name}_{i}";
            }

            _names[layer] = unique;
        }
    }

    public static MvtLayerNames FromMap(IServiceMap map) => new(map.TOC, map.MapElements);

    public string? this[ILayer layer] => _names.TryGetValue(layer, out var name) ? name : null;

    public IReadOnlyDictionary<ILayer, string> All => _names;

    public static string Normalize(string? name)
    {
        if (String.IsNullOrWhiteSpace(name))
        {
            return "layer";
        }

        var sb = new StringBuilder(name.Length);
        foreach (char c in name.Trim().ToLowerInvariant())
        {
            sb.Append(c switch
            {
                'ä' => "ae",
                'ö' => "oe",
                'ü' => "ue",
                'ß' => "ss",
                _ when (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') => c.ToString(),
                _ => "_"
            });
        }

        string result = sb.ToString();
        while (result.Contains("__"))
        {
            result = result.Replace("__", "_");
        }

        result = result.Trim('_');
        return result.Length == 0 ? "layer" : result;
    }

    private static string TocPath(ITocElement element)
    {
        var parts = new List<string>();
        for (var e = element; e != null; e = e.ParentGroup)
        {
            parts.Insert(0, e.Name);
        }

        return String.Join("_", parts);
    }
}
