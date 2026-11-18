using System.Globalization;
using System.Text.RegularExpressions;

namespace gView.Framework.NetworkModel.Graph;

/// <summary>The @x,y[~radius] node references in a list of references (see <see cref="FileNetworkGraph.ResolveNodes"/>).</summary>
public static partial class NetworkLocations
{
    [GeneratedRegex(@"@\s*([-+]?[0-9]*\.?[0-9]+(?:[eE][-+]?[0-9]+)?)\s*,\s*([-+]?[0-9]*\.?[0-9]+(?:[eE][-+]?[0-9]+)?)")]
    private static partial Regex LocationRegex();

    /// <summary>A location reference for a point, e.g. "@1234.5,5678.25".</summary>
    public static string Reference(double x, double y)
        => $"@{x.ToString("R", CultureInfo.InvariantCulture)},{y.ToString("R", CultureInfo.InvariantCulture)}";

    /// <summary>Replaces the coordinates of every @x,y in the references (e.g. projects them into the spatial reference of the network); the radius stays.</summary>
    public static string Transform(string references, Func<double, double, (double X, double Y)> transform)
        => LocationRegex().Replace(references, m =>
        {
            var (x, y) = transform(
                double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));
            return Reference(x, y);
        });
}
