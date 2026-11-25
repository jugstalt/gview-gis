using gView.Interoperability.OgcApi.GeoJson;
using System.Security.Cryptography;
using System.Text.Json;

namespace gView.Interoperability.OgcApi.Features;

/// <summary>
/// The entity tag of a feature for optimistic locking (OGC API - Features Part 4):
/// GET .../items/{featureId} returns it in the <c>ETag</c> header, PUT/PATCH/DELETE with an
/// <c>If-Match</c> header only change the feature, if it still has that tag (otherwise 412).
/// <para>
/// The tag is a hash of the id, the geometry in CRS84 and the properties of the feature, so it
/// is the same for every output CRS and format of the item (the CRS of the response is not part
/// of the feature's state). It is a strong tag, because <c>If-Match</c> compares strong tags only.
/// </para>
/// </summary>
public sealed class FeatureETag : IEquatable<FeatureETag>
{
    private FeatureETag(string value) => Value = value;

    /// <summary>The quoted tag as written in the <c>ETag</c> header, e.g. <c>"q3Jd…"</c></summary>
    public string Value { get; }

    /// <param name="crs84Feature">the feature queried in CRS84 (<see cref="CrsReference.Crs84"/>)</param>
    public static FeatureETag Compute(OgcFeature crs84Feature)
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms, GeoJsonWriter.WriterOptions))
        {
            GeoJsonWriter.WriteFeature(writer, crs84Feature, swapAxes: false);
        }

        var hash = SHA256.HashData(ms.ToArray());
        var tag = Convert.ToBase64String(hash, 0, 18)  // 144 bit, no padding
            .Replace('+', '-')
            .Replace('/', '_');

        return new FeatureETag($"\"{tag}\"");
    }

    /// <summary>
    /// True, if an <c>If-Match</c> header allows changing the feature: no header, <c>*</c>, or one
    /// of the listed tags is this tag. Weak tags (<c>W/"…"</c>) never match (strong comparison, RFC 9110).
    /// </summary>
    public bool MatchesIfMatch(string? ifMatch)
    {
        if (String.IsNullOrWhiteSpace(ifMatch))
        {
            return true;
        }

        return ifMatch.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                      .Any(tag => tag == "*" || tag == Value);
    }

    public bool Equals(FeatureETag? other) => other is not null && other.Value == Value;

    public override bool Equals(object? obj) => Equals(obj as FeatureETag);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;
}
