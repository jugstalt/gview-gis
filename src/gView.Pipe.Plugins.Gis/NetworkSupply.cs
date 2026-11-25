using System.Globalization;
using System.Text.RegularExpressions;

namespace gView.Pipe.Plugins.Gis;

/// <summary>
/// The node labels of a search result as one text, cheapest first: "Transformer#4711 Transformer#4712". Layer names
/// may contain blanks ("Low Voltage/House Connection#5"): the OIDs are read after the '#', not by splitting at blanks.
/// </summary>
internal readonly partial record struct NodeLabels(string? Text)
{
    private IEnumerable<long> Oids => OidPattern().Matches(Text ?? "")
        .Select(m => long.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var oid) ? oid : (long?)null)
        .OfType<long>();

    /// <summary>The OID of the first label ("Transformer#4711 Transformer#4712" → 4711).</summary>
    public long? FirstOid => Oids.Select(oid => (long?)oid).FirstOrDefault();

    /// <summary>The OIDs of all labels as text, in their order ("Transformer#4711 Transformer#4712" → "4711,4712").</summary>
    public string? OidsText
    {
        get
        {
            var oids = Oids
                .Distinct()
                .Select(oid => oid.ToString(CultureInfo.InvariantCulture))
                .ToList();
            return oids.Count == 0 ? null : string.Join(",", oids);
        }
    }

    public override string? ToString() => Text;

    /// <summary>The OID of a label: digits after a '#', up to a blank, a part ("/0") or the end.</summary>
    [GeneratedRegex(@"#(\d+)(?=\s|/|$)")]
    private static partial Regex OidPattern();
}

/// <summary>The kinds of a line in a feeders result.</summary>
internal static class SupplyKind
{
    /// <summary>One branch feeds the line.</summary>
    public const string Edge = "Edge";

    /// <summary>More than one branch: a closed ring, double supply.</summary>
    public const string Multiple = "Multiple";

    /// <summary>No branch went along the line, e.g. switched off at the separation point of a ring.</summary>
    public const string Open = "Open";

    /// <summary>The kind of a line from the kinds of its parts: Multiple wins, Open only when every part is open.</summary>
    public static string Of(IEnumerable<string> partKinds)
    {
        var kinds = partKinds.ToList();
        return kinds.Contains(Multiple) ? Multiple : kinds.Count > 0 && kinds.All(k => k == Open) ? Open : Edge;
    }
}

/// <summary>What feeds a line: its kind and the sources and branches it hangs on.</summary>
internal sealed record NetworkSupply(string Kind, NodeLabels Sources, NodeLabels Branches);
