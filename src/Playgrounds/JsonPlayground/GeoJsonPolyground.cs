using gView.Framework.OGC.GeoJson;
using JsonPlayground.Models.NGeoJson;
using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;


namespace JsonPlayground;

internal class GeoJsonPolyground
{
    static public void Do()
    {
        string geoJsonString = """
            {
              "type": "FeatureCollection",
              "features": [
                {
                  "type": "Feature",
                  "geometry": {
                    "type": "Point",
                    "coordinates": [102.0, 0.5]
                  },
                  "properties": {
                    "prop0": "value0"
                  }
                },
                {
                  "type": "Feature",
                  "geometry": {
                    "type": "LineString",
                    "coordinates": [
                      [102.0, 0.0],
                      [103.0, 1.0],
                      [104.0, 0.0],
                      [105.0, 1.0]
                    ]
                  },
                  "properties": {
                    "prop0": "value0",
                    "prop1": 0.0
                  }
                },
                {
                  "type": "Feature",
                  "geometry": {
                    "type": "MultiLineString",
                    "coordinates": [
                        [
                      [102.0, 0.0],
                      [103.0, 1.0],
                      [104.0, 0.0],
                      [105.0, 1.0]
                        ],
                        [
                      [202.0, 0.0],
                      [203.0, 1.0],
                      [204.0, 0.0],
                      [205.0, 1.0]
                        ]
                    ]
                  },
                  "properties": {
                    "prop0": "value0",
                    "prop1": 0.0
                  }
                },
                {
                  "type": "Feature",
                  "geometry": {
                    "type": "Polygon",
                    "coordinates": [
                      [
                        [100.0, 0.0],
                        [101.0, 0.0],
                        [101.0, 1.0],
                        [100.0, 1.0],
                        [100.0, 0.0]
                      ]
                    ]
                  },
                  "properties": {
                    "prop0": "value0",
                    "prop1": { "this": "that" }
                  }
                },
            {
                  "type": "Feature",
                  "geometry": {
                    "type": "Polygon",
                    "coordinates": [
                        [
                      [102.0, 0.0],
                      [103.0, 1.0],
                      [104.0, 0.0],
                      [105.0, 1.0]
                        ],
                        [
                      [202.0, 0.0],
                      [203.0, 1.0],
                      [204.0, 0.0],
                      [205.0, 1.0]
                        ]
                    ]
                  },
                  "properties": {
                    "prop0": "value0",
                    "prop1": { 
                        "this": "that",
                        "double" : 1.2,
                        "int": 42,
                        "bool1": true,
                        "bool2": false
                    }
                  }
                }
              ]
            }

            """;

        var NgeoJson = Newtonsoft.Json.JsonConvert.DeserializeObject<NGeoJsonFeatures>(geoJsonString);
        var geoJson = JsonSerializer.Deserialize<GeoJsonFeatures>(geoJsonString);

        var Nserialized = Newtonsoft.Json.JsonConvert.SerializeObject(NgeoJson).Replace(".0","");
        var serialized = JsonSerializer.Serialize(geoJson).Replace(".0","");

        Console.WriteLine(Nserialized);
        Console.WriteLine(serialized);
        Console.WriteLine($"Ident: {Nserialized == serialized}");
        Console.WriteLine("------------------------------------------------------------------------------");

        foreach (var feature in NgeoJson.Features)
        {
            var geometry = feature.ToGeometry();
            Console.WriteLine(geometry.GetType().Name);
            feature.Geometry = null;
            feature.FromShape(geometry);
        }
        foreach (var feature in geoJson.Features)
        {
            var geometry = feature.ToGeometry();
            Console.WriteLine(geometry.GetType().Name);
            feature.Geometry = null;
            feature.FromShape(geometry);
        }

        var Nserialized_ = Newtonsoft.Json.JsonConvert.SerializeObject(NgeoJson).Replace(".0", "");
        var serialized_ = JsonSerializer.Serialize(geoJson).Replace(".0", "");

        Console.WriteLine(Nserialized_);
        Console.WriteLine(serialized_);
        Console.WriteLine($"Ident: {Nserialized == Nserialized_}");
        Console.WriteLine($"Ident: {serialized == serialized_}");
        Console.WriteLine("------------------------------------------------------------------------------");

        var Nfeature4 = NgeoJson.Features[4];
        var feature4 = geoJson.Features[4];

        Console.WriteLine(Nfeature4.GetPropery<string>("prop0"));
        //Console.WriteLine(Nfeature4.GetPropery<object>("prop1").ToString());
        Console.WriteLine(Nfeature4.GetPropery<string>("prop1.this").ToString());
        Console.WriteLine(Nfeature4.GetPropery<string>("prop1.int").ToString());
        Console.WriteLine(Nfeature4.GetPropery<string>("prop1.double").ToString());
        Console.WriteLine(Nfeature4.GetPropery<string>("prop1.bool1").ToString());
        Console.WriteLine(Nfeature4.GetPropery<string>("prop1.bool2").ToString());
        Console.WriteLine("-------");
        Console.WriteLine(feature4.GetPropery<string>("prop0"));
        //Console.WriteLine(Nfeature4.GetPropery<object>("prop1").ToString());
        Console.WriteLine(feature4.GetPropery<string>("prop1.this").ToString());
        Console.WriteLine(feature4.GetPropery<string>("prop1.int").ToString());
        Console.WriteLine(feature4.GetPropery<string>("prop1.double").ToString());
        Console.WriteLine(feature4.GetPropery<string>("prop1.bool1").ToString());
        Console.WriteLine(feature4.GetPropery<string>("prop1.bool2").ToString());
    }
}
