using gView.Framework.Cartography.Rendering;
using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Core.Carto;

namespace gView.Framework.Cartography.VectorTiles;

/// <summary>
/// The labels of a map layer in vector tiles: an extra MVT layer <c>{layer}__labels</c> whose features carry the
/// label text computed by gView (field, expression, script) as <c>_label{index}</c>, one attribute per label
/// renderer (a scale dependent label renderer has several).
/// </summary>
/// <remarks>
/// Points and polygons get one label point (polygons: gView's polygon label point), written only into the tile
/// that contains it, so a label is not repeated in every tile. Lines keep their (clipped) geometry, the client
/// places the text along it.
/// </remarks>
public static class MvtLabels
{
    public const string LayerSuffix = "__labels";
    public const string AttributePrefix = "_label";

    public static string LayerName(string mvtLayerName) => mvtLayerName + LayerSuffix;

    public static string Attribute(int index) => AttributePrefix + index;

    /// <summary>
    /// The label renderers that can be expressed in vector tiles, with their scale range
    /// (denominators, values &lt;= 1 = unlimited) and the index of their text attribute.
    /// </summary>
    public static IReadOnlyList<LabelEntry> Entries(ILabelRenderer? renderer)
    {
        var result = new List<LabelEntry>();

        switch (renderer)
        {
            case SimpleLabelRenderer simple:  // AdvancedLabelRenderer too
                result.Add(new LabelEntry(0, simple, 0, 0));
                break;

            case ScaleDependentLabelRenderer scaleDependent:
                foreach (var item in scaleDependent.RendererItems)
                {
                    if (item is IScaledependent range && item.Renderer is SimpleLabelRenderer inner)
                    {
                        result.Add(new LabelEntry(result.Count, inner, range.MinimumScale, range.MaximumScale));
                    }
                }
                break;
        }

        return result;
    }

    public sealed record LabelEntry(int Index, SimpleLabelRenderer Renderer, double MinimumScale, double MaximumScale)
    {
        public string Attribute => MvtLabels.Attribute(Index);

        public bool InScale(double scale)
            => !(MinimumScale > 1 && MinimumScale > scale) && !(MaximumScale > 1 && MaximumScale < scale);
    }
}
