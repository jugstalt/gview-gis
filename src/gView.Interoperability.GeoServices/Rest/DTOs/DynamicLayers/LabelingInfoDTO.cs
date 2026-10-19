using gView.Framework.Cartography.Rendering;
using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.DynamicLayers
{
    public class LabelingInfoDTO
    {
        [JsonPropertyName("labelPlacement")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string LabelPlacement { get; set; }

        [JsonPropertyName("labelExpression")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string LabelExpression { get; set; }

        [JsonPropertyName("useCodedValues")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? UseCodedValues { get; set; }

        [JsonPropertyName("symbol")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Symbol { get; set; }

        [JsonPropertyName("minScale")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MinScale { get; set; }

        [JsonPropertyName("maxScale")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MaxScale { get; set; }

        [JsonPropertyName("where")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Where { get; set; }

        public static string DefaultLabelPlacement(GeometryType type)
        {
            switch (type)
            {
                case GeometryType.Point:
                    return "esriServerPointLabelPlacementAboveRight";
                case GeometryType.Polyline:
                    return "esriServerLinePlacementAboveAlong";
                case GeometryType.Polygon:
                    return "esriServerPolygonPlacementAlwaysHorizontal";
            }

            return String.Empty;
        }

        #region Members

        public ILabelRenderer ToLabelRenderer()
        {
            var labelRenderer = new SimpleLabelRenderer();

            if (!String.IsNullOrWhiteSpace(LabelExpression))
            {
                labelRenderer.UseExpression = true;
                labelRenderer.LabelExpression = LabelExpression;
            }
            labelRenderer.TextSymbol = this.Symbol switch
            {
                JsonElement jsonElement => Renderers.SimpleRenderers.JsonRendererDTO.FromSymbolJObject(jsonElement) as ITextSymbol,
                _ => null
            };

            if (labelRenderer.TextSymbol != null
                && TextAlignmentFromPlacement(LabelPlacement) is TextSymbolAlignment alignment)
            {
                labelRenderer.TextSymbol.TextSymbolAlignment = alignment;
            }

            // ArcGIS: minScale = zoomed out limit, maxScale = zoomed in limit (0 = unlimited).
            // gView: MinimumScale = zoomed in limit, MaximumScale = zoomed out limit.
            if (MinScale.GetValueOrDefault() > 0 || MaxScale.GetValueOrDefault() > 0)
            {
                var scaleRenderer = new ScaleDependentLabelRenderer();
                if (scaleRenderer.RendererItems.Create(labelRenderer) is IScaledependent scaleItem)
                {
                    scaleItem.MinimumScale = MaxScale.GetValueOrDefault();
                    scaleItem.MaximumScale = MinScale.GetValueOrDefault();
                    scaleRenderer.RendererItems.Add((IRendererGroupItem)scaleItem);

                    return scaleRenderer;
                }
            }

            return labelRenderer;
        }

        /// <summary>
        /// Maps esriServer(Point|Line)...Placement values (Above|Below|Center + Left|Center|Right) to a text alignment.
        /// Returns null for unknown placements (symbol alignment is kept).
        /// </summary>
        static public TextSymbolAlignment? TextAlignmentFromPlacement(string placement)
        {
            if (String.IsNullOrEmpty(placement))
            {
                return null;
            }

            var vertical = placement.Contains("Above") ? 0 : placement.Contains("Below") ? 2 : placement.Contains("Center") ? 1 : -1;

            if (placement.StartsWith("esriServerPointLabelPlacement"))
            {
                var horizontal = placement.EndsWith("Left") ? 0 : placement.EndsWith("Right") ? 2 : placement.EndsWith("Center") ? 1 : -1;

                // placement left of the point: text ends at the point (right aligned), and vice versa
                return (vertical, horizontal) switch
                {
                    (0, 0) => TextSymbolAlignment.rightAlignOver,
                    (0, 1) => TextSymbolAlignment.Over,
                    (0, 2) => TextSymbolAlignment.leftAlignOver,
                    (1, 0) => TextSymbolAlignment.rightAlignCenter,
                    (1, 1) => TextSymbolAlignment.Center,
                    (1, 2) => TextSymbolAlignment.leftAlignCenter,
                    (2, 0) => TextSymbolAlignment.rightAlignUnder,
                    (2, 1) => TextSymbolAlignment.Under,
                    (2, 2) => TextSymbolAlignment.leftAlignUnder,
                    _ => null
                };
            }

            if (placement.StartsWith("esriServerLinePlacement"))
            {
                return vertical switch
                {
                    0 => TextSymbolAlignment.Over,
                    1 => TextSymbolAlignment.Center,
                    2 => TextSymbolAlignment.Under,
                    _ => null
                };
            }

            return null;
        }

        #endregion
    }
}

/*
 * 
Label Placement Values For Point Features
esriServerPointLabelPlacementAboveCenter 	esriServerPointLabelPlacementAboveLeft 	esriServerPointLabelPlacementAboveRight
esriServerPointLabelPlacementBelowCenter 	esriServerPointLabelPlacementBelowLeft 	esriServerPointLabelPlacementBelowRight
esriServerPointLabelPlacementCenterCenter 	esriServerPointLabelPlacementCenterLeft 	esriServerPointLabelPlacementCenterRight
 * 
Label Placement Values For Line Features
esriServerLinePlacementAboveAfter 	esriServerLinePlacementAboveAlong 	esriServerLinePlacementAboveBefore
esriServerLinePlacementAboveStart 	esriServerLinePlacementAboveEnd 	 
esriServerLinePlacementBelowAfter 	esriServerLinePlacementBelowAlong 	esriServerLinePlacementBelowBefore
esriServerLinePlacementBelowStart 	esriServerLinePlacementBelowEnd 	 
esriServerLinePlacementCenterAfter 	esriServerLinePlacementCenterAlong 	esriServerLinePlacementCenterBefore
esriServerLinePlacementCenterStart 	esriServerLinePlacementCenterEnd 	 
 * 
Label Placement Values For Polygon Features
esriServerPolygonPlacementAlwaysHorizontal
 
 * 
 * 
 *  Example
{
    "labelPlacement": "esriServerPointLabelPlacementAboveRight",
    "labelExpression": "[NAME]",
    "useCodedValues": false,
    "symbol": {
     "type": "esriTS",
     "color": [38,115,0,255],
     "backgroundColor": null,
     "borderLineColor": null,
     "verticalAlignment": "bottom",
     "horizontalAlignment": "left",
     "rightToLeft": false,
     "angle": 0,
     "xoffset": 0,
     "yoffset": 0,
     "kerning": true,
     "font": {
      "family": "Arial",
      "size": 11,
      "style": "normal",
      "weight": "bold",
      "decoration": "none"
     }
    },
    "minScale": 0,
    "maxScale": 0,
    "where" : "NAME LIKE 'A%'" //label only those feature where name begins with A
 } 
 * 
 */


