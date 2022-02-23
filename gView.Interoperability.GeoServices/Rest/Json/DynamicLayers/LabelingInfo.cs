using gView.Framework.Carto;
using gView.Framework.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.DynamicLayers
{
    public class LabelingInfo
    {
        [JsonProperty("labelPlacement", NullValueHandling = NullValueHandling.Ignore)]
        public string LabelPlacement { get; set; }

        [JsonProperty("labelExpression", NullValueHandling = NullValueHandling.Ignore)]
        public string LabelExpression { get; set; }

        [JsonProperty("useCodedValues", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseCodedValues { get; set; }

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public object Symbol { get; set; }

        [JsonProperty("minScale", NullValueHandling = NullValueHandling.Ignore)]
        public double? MinScale { get; set; }

        [JsonProperty("maxScale", NullValueHandling = NullValueHandling.Ignore)]
        public double? MaxScale { get; set; }

        [JsonProperty("where", NullValueHandling = NullValueHandling.Ignore)]
        public string Where { get; set; }

        public static string DefaultLabelPlacement(geometryType type)
        {
            switch (type)
            {
                case geometryType.Point:
                    return "esriServerPointLabelPlacementAboveRight";
                case geometryType.Polyline:
                    return "esriServerLinePlacementAboveAlong";
                case geometryType.Polygon:
                    return "esriServerPolygonPlacementAlwaysHorizontal";
            }

            return String.Empty;
        }

        #region Members

        public ILabelRenderer ToLabelRenderer()
        {
            var labelRenderer = new Framework.Carto.Rendering.SimpleLabelRenderer();

            if (!String.IsNullOrWhiteSpace(LabelExpression))
            {
                labelRenderer.UseExpression = true;
                labelRenderer.LabelExpression = LabelExpression;
            }
            labelRenderer.TextSymbol = Renderers.SimpleRenderers.JsonRenderer.FromSymbolJObject(Symbol as JObject) as Framework.Symbology.ITextSymbol;

            return labelRenderer;
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


