using gView.GraphicsEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

static internal class GraphicsExtensions
{
    static private Dictionary<ImageFormat, string> ContentTypes =
        new Dictionary<ImageFormat, string>(
            Enum.GetValues<ImageFormat>()
                .Select(f => f switch {
                    ImageFormat.Jpeg => new KeyValuePair<ImageFormat, string>(f, $"image/jpg"),
                    _ => new KeyValuePair<ImageFormat, string>(f, $"image/{f.ToString().ToLowerInvariant()}")
                 })
            );

    static public string ToContentType(this ImageFormat format)
        => ContentTypes[format];
}
