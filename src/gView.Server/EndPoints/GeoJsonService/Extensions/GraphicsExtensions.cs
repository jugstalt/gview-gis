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
                .Select(f => new KeyValuePair<ImageFormat, string>(f, $"image/{f.ToString().ToLower()}"))
            );

    static public string ToContentType(this ImageFormat format)
        => ContentTypes[format];
}
