using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// A label class associated with a feature layer.
/// </summary>
internal class CimLabelClass
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Label expression, e.g. a field name like "STRNAME" or an Arcade / VBScript expression.
    /// </summary>
    [JsonPropertyName("expression")]
    public string? Expression { get; set; }

    [JsonPropertyName("expressionEngine")]
    public string? ExpressionEngine { get; set; }

    [JsonPropertyName("fieldNames")]
    public List<string>? FieldNames { get; set; }

    [JsonPropertyName("textSymbol")]
    public CimSymbolReference? TextSymbol { get; set; }

    /// <summary>SQL where clause to restrict labeling, e.g. "STATUS = 'Active'".</summary>
    [JsonPropertyName("whereClause")]
    public string? WhereClause { get; set; }

    [JsonPropertyName("visibility")]
    public bool Visibility { get; set; } = true;

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("minimumScale")]
    public double MinimumScale { get; set; }

    [JsonPropertyName("maximumScale")]
    public double MaximumScale { get; set; }

    /// <summary>
    /// Present when the map uses the (older) Standard label engine
    /// (<see cref="CimGeneralPlacementProperties"/>) - always populated by ArcGIS Pro
    /// regardless of which engine is actually active, alongside
    /// <see cref="MaplexLabelPlacementProperties"/>.
    /// </summary>
    [JsonPropertyName("standardLabelPlacementProperties")]
    public CimStandardLabelPlacementProperties? StandardLabelPlacementProperties { get; set; }

    /// <summary>Present when the map uses the (default, modern) Maplex label engine.</summary>
    [JsonPropertyName("maplexLabelPlacementProperties")]
    public CimMaplexLabelPlacementProperties? MaplexLabelPlacementProperties { get; set; }
}

internal class CimStandardLabelPlacementProperties
{
    /// <summary>"OneLabelPerName", "OneLabelPerFeature"/"OneLabelPerShape", or "OneLabelPerPart".</summary>
    [JsonPropertyName("numLabelsOption")]
    public string? NumLabelsOption { get; set; }

    [JsonPropertyName("pointPlacementMethod")]
    public string? PointPlacementMethod { get; set; }

    /// <summary>
    /// Priority per zone around the point (lower = placed first); ties are broken by the
    /// fixed zone order below. Only meaningful when <see cref="PointPlacementMethod"/> is
    /// "AroundPoint" (the standard/default method for point features).
    /// </summary>
    [JsonPropertyName("pointPlacementPriorities")]
    public CimPointZonePriorities? PointPlacementPriorities { get; set; }

    /// <summary>Field carrying each feature's own rotation angle. Only meaningful when
    /// <see cref="PointPlacementMethod"/> is "RotationField".</summary>
    [JsonPropertyName("rotationField")]
    public string? RotationField { get; set; }

    /// <summary>Angle convention of <see cref="RotationField"/>'s values - "Arithmetic" or
    /// "Geographic".</summary>
    [JsonPropertyName("rotationType")]
    public string? RotationType { get; set; }

    /// <summary>Which side(s) of a line a label may be placed on.</summary>
    [JsonPropertyName("lineLabelPosition")]
    public CimStandardLineLabelPosition? LineLabelPosition { get; set; }

    /// <summary>
    /// ArcGIS Pro's per-label-class "Allow overlapping labels" checkbox - this label class is
    /// exempt from the overlap check entirely instead of being dropped when it would collide
    /// with another already-placed label.
    /// </summary>
    [JsonPropertyName("allowOverlappingLabels")]
    public bool AllowOverlappingLabels { get; set; }

    /// <summary>
    /// ArcGIS Pro's per-label-class "Feature weight" (Label Priority Ranking dialog / Placement
    /// Properties tab) - how strongly this layer's own feature geometry blocks *other* labels
    /// (from any layer) from being placed on top of it. "None" (the default) means features
    /// don't block labels at all; otherwise "Low", "Medium", or "High".
    /// </summary>
    [JsonPropertyName("featureWeight")]
    public string? FeatureWeight { get; set; }
}

/// <summary>
/// CIMStandardLineLabelPosition: simple on/off gates for line label placement (as opposed to
/// the point zones' numeric ranking). ArcGIS Pro omits properties left at their default
/// (false) from the JSON, so absence here means "not allowed", same as an explicit false.
/// </summary>
internal class CimStandardLineLabelPosition
{
    [JsonPropertyName("above")]
    public bool Above { get; set; }

    [JsonPropertyName("below")]
    public bool Below { get; set; }

    /// <summary>Centered on the line.</summary>
    [JsonPropertyName("inLine")]
    public bool InLine { get; set; }

    /// <summary>Text follows the line's direction rather than staying horizontal.</summary>
    [JsonPropertyName("parallel")]
    public bool Parallel { get; set; }
}

internal class CimMaplexLabelPlacementProperties
{
    [JsonPropertyName("pointPlacementMethod")]
    public string? PointPlacementMethod { get; set; }

    /// <summary>Same shape/semantics as <see cref="CimStandardLabelPlacementProperties.PointPlacementPriorities"/>.</summary>
    [JsonPropertyName("pointExternalZonePriorities")]
    public CimPointZonePriorities? PointExternalZonePriorities { get; set; }

    /// <summary>
    /// Maplex's "Feature weight" - unlike Standard's None/Low/Medium/High enum (see
    /// <see cref="CimStandardLabelPlacementProperties.FeatureWeight"/>), this is a continuous
    /// 0-1000 scale (0 = not weighted, matching Standard's "None"). Same purpose: how strongly
    /// this layer's own feature geometry blocks *other* labels from being placed on top of it.
    /// Only takes effect when <see cref="EnableFeatureWeight"/> is true.
    /// </summary>
    [JsonPropertyName("featureWeight")]
    public double? FeatureWeight { get; set; }

    /// <summary>
    /// Gates <see cref="FeatureWeight"/> - ArcGIS Pro only applies the feature weight when this
    /// is explicitly true. Not observed set to true in any real aprx checked so far (absent
    /// entirely, in fact - see the FeatureLabelPriority conversion for the resulting caveat).
    /// </summary>
    [JsonPropertyName("enableFeatureWeight")]
    public bool EnableFeatureWeight { get; set; }

    /// <summary>
    /// Maplex's separate 0-1000 weight for a *polygon's boundary* specifically (as opposed to
    /// its interior, covered by <see cref="FeatureWeight"/>). Not currently converted - always
    /// 0 (unset) in every real aprx seen so far, so there's no real-world signal to convert yet.
    /// </summary>
    [JsonPropertyName("polygonBoundaryWeight")]
    public double? PolygonBoundaryWeight { get; set; }
}

/// <summary>
/// Placement priority for each of the 8 zones around a point label anchor (both the Maplex
/// and Standard label engines use this exact shape, just under different property names on
/// their respective placement-properties objects). Lower value = tried first.
/// </summary>
internal class CimPointZonePriorities
{
    [JsonPropertyName("aboveLeft")]
    public int AboveLeft { get; set; }

    [JsonPropertyName("aboveCenter")]
    public int AboveCenter { get; set; }

    [JsonPropertyName("aboveRight")]
    public int AboveRight { get; set; }

    [JsonPropertyName("centerLeft")]
    public int CenterLeft { get; set; }

    [JsonPropertyName("centerRight")]
    public int CenterRight { get; set; }

    [JsonPropertyName("belowLeft")]
    public int BelowLeft { get; set; }

    [JsonPropertyName("belowCenter")]
    public int BelowCenter { get; set; }

    [JsonPropertyName("belowRight")]
    public int BelowRight { get; set; }
}
