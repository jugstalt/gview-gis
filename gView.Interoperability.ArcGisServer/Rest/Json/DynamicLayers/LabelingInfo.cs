using gView.Framework.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.DynamicLayers
{
    class LabelingInfo
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
        public int? MinScale { get; set; }

        [JsonProperty("maxScale", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxScale { get; set; }

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


