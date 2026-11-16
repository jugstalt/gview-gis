using System.Text;

namespace gView.Framework.NetworkModel.Templates;

/// <summary>
/// A text with placeholders: [FIELD] for an attribute, {layer-name}, {group-name} (the direct group) and
/// {group-name:n} (the n-th group from the top, 1-based). Parsed once, evaluated per feature.
/// </summary>
public sealed class ValueTemplate
{
    private abstract record Part;
    private sealed record Text(string Value) : Part;
    private sealed record Field(string Name) : Part;

    private readonly Part[] _parts;

    private ValueTemplate(Part[] parts) => _parts = parts;

    public IEnumerable<string> FieldNames => _parts.OfType<Field>().Select(f => f.Name);

    public bool IsEmpty => _parts.Length == 0;

    /// <summary>Parses the template and resolves the layer and group placeholders, which are the same for every feature.</summary>
    public static ValueTemplate Parse(string? template, string layerName, IReadOnlyList<string> groupPath)
    {
        var parts = new List<Part>();
        var text = new StringBuilder();
        template ??= "";

        void FlushText()
        {
            if (text.Length > 0)
            {
                parts.Add(new Text(text.ToString()));
                text.Clear();
            }
        }

        for (int i = 0; i < template.Length; i++)
        {
            char c = template[i];
            if (c == '[' || c == '{')
            {
                char close = c == '[' ? ']' : '}';
                int end = template.IndexOf(close, i + 1);
                if (end < 0)
                {
                    throw new FormatException($"Missing '{close}' in template '{template}'.");
                }
                string name = template.Substring(i + 1, end - i - 1).Trim();
                if (c == '[')
                {
                    FlushText();
                    parts.Add(new Field(name));
                }
                else
                {
                    text.Append(ResolvePlaceholder(name, layerName, groupPath, template));
                }
                i = end;
            }
            else
            {
                text.Append(c);
            }
        }
        FlushText();

        return new ValueTemplate(parts.ToArray());
    }

    private static string ResolvePlaceholder(string name, string layerName, IReadOnlyList<string> groupPath, string template)
    {
        var lower = name.ToLowerInvariant();
        if (lower == "layer-name")
        {
            return layerName;
        }
        if (lower == "group-name")
        {
            return groupPath.Count > 0 ? groupPath[^1] : "";
        }
        if (lower.StartsWith("group-name:") && int.TryParse(lower["group-name:".Length..], out int level) && level >= 1)
        {
            return level <= groupPath.Count ? groupPath[level - 1] : "";
        }
        throw new FormatException($"Unknown placeholder {{{name}}} in template '{template}'.");
    }

    /// <summary>Evaluates the template; <paramref name="fieldValue"/> gives the text of an attribute (null or empty when not set).</summary>
    public string Evaluate(Func<string, string?> fieldValue)
    {
        if (_parts.Length == 1)
        {
            return _parts[0] switch
            {
                Text t => t.Value,
                Field f => fieldValue(f.Name)?.Trim() ?? "",
                _ => ""
            };
        }

        var sb = new StringBuilder();
        foreach (var part in _parts)
        {
            sb.Append(part switch
            {
                Text t => t.Value,
                Field f => fieldValue(f.Name)?.Trim() ?? "",
                _ => ""
            });
        }
        return sb.ToString().Trim();
    }
}
