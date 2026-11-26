using gView.Framework.Core.Geometry;
using SpatialNET.IO;
using System;
using System.Collections.Generic;

namespace gView.DataSources.VectorTileCache;

/// <summary>A feature decoded from a vector tile: geometry in WGS84 and the MVT attributes.</summary>
internal sealed record VectorTileFeature(IGeometry Shape, IReadOnlyList<KeyValuePair<string, MvtValue>> Attributes, ulong? Id);

/// <summary>Decodes Mapbox vector tiles (MVT, gzipped or not) of the WebMercator XYZ grid with SpatialNET.IO.</summary>
internal static class MvtTileReader
{
    private static readonly TileGrid WebMercatorGrid = TileGrid.WebMercator(30);

    /// <summary>Decodes the tile <paramref name="level"/>/<paramref name="col"/>/<paramref name="row"/> (XYZ, row from the top).</summary>
    /// <returns>The features per MVT layer name; layers without features are left out.</returns>
    public static IReadOnlyDictionary<string, List<VectorTileFeature>> Read(ReadOnlySpan<byte> data, int level, int col, int row)
    {
        var result = new Dictionary<string, List<VectorTileFeature>>();
        if (data.Length == 0)
        {
            return result;  // an empty tile
        }

        var tile = Mvt.Decode(data);

        foreach (var layer in tile.Layers)
        {
            if (layer.Features.Count == 0)
            {
                continue;
            }

            var transform = new MvtTileTransform(WebMercatorGrid, level, row, col, layer.Extent);
            var features = new List<VectorTileFeature>(layer.Features.Count);

            foreach (var mvtFeature in layer.Features)
            {
                if (mvtFeature.ToGeometry(Wgs84GeometryFactory.Instance, transform) is not IGeometry shape)
                {
                    continue;
                }

                features.Add(new VectorTileFeature(shape, mvtFeature.Attributes, mvtFeature.Id));
            }

            if (features.Count > 0)
            {
                if (result.TryGetValue(layer.Name, out var existing))
                {
                    existing.AddRange(features);  // a layer name used twice in one tile
                }
                else
                {
                    result[layer.Name] = features;
                }
            }
        }

        return result;
    }
}
