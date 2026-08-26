using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// CIM annotation layer definition (type = "CIMAnnotationLayer").
/// Unlike <see cref="CimFeatureLayer"/>, an annotation layer has no renderer/labelClasses of
/// its own - each feature carries its own rendered text, position and rotation directly as
/// attribute columns (TextString/Angle/FontName/...; this is the older, field-based ArcGIS
/// annotation schema, as opposed to per-feature binary graphic overrides). The converter maps
/// this onto a regular FeatureLayer with a label renderer driven by those columns instead of
/// a CIM label class.
/// </summary>
internal class CimAnnotationLayer : CimBaseLayer
{
    [JsonPropertyName("featureTable")]
    public CimFeatureTable? FeatureTable { get; set; }

    /// <summary>Minimum display scale (0 = no restriction).</summary>
    [JsonPropertyName("minScale")]
    public double MinScale { get; set; }

    /// <summary>Maximum display scale (0 = no restriction).</summary>
    [JsonPropertyName("maxScale")]
    public double MaxScale { get; set; }

    [JsonPropertyName("serviceLayerID")]
    public int ServiceLayerId { get; set; }

    /// <summary>Layer transparency, 0 (opaque, the default) - 100 (fully invisible).</summary>
    [JsonPropertyName("transparency")]
    public double Transparency { get; set; }

    /// <summary>
    /// Whether this layer's labels scale with the map's reference scale - ArcGIS Pro's Layer
    /// Properties -> General -> "Scale symbols when a reference scale is set" checkbox. See
    /// <see cref="CimFeatureLayer.ScaleSymbols"/> for the full explanation.
    /// </summary>
    [JsonPropertyName("scaleSymbols")]
    public bool ScaleSymbols { get; set; } = true;

    /// <summary>
    /// One entry per annotation class ("Standard" unless the author defined more in ArcGIS
    /// Pro). ArcGIS Server/Pro publish each of these as its own child layer nested under a
    /// group named after the annotation layer itself - not as one flat layer - so the
    /// converter mirrors that instead of producing a single merged layer.
    /// </summary>
    [JsonPropertyName("subLayers")]
    public List<CimAnnotationSubLayer>? SubLayers { get; set; }
}

internal class CimAnnotationSubLayer
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Matches the feature table's "AnnotationClassID" column - features belong to this
    /// sub-layer/class when that column equals this value.
    /// </summary>
    [JsonPropertyName("subLayerID")]
    public string? SubLayerId { get; set; }

    [JsonPropertyName("visibility")]
    public bool Visibility { get; set; } = true;

    [JsonPropertyName("serviceLayerID")]
    public int ServiceLayerId { get; set; }
}
