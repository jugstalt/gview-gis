using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;

namespace gView.Server.AppCode.Extensions
{
    static public class GerneralExtensions
    {
        static public string ServiceName(this string id)
        {
            if (id.Contains("@"))
            {
                return id.Split('@')[1].Trim();
            }
            else if (id.Contains("/"))
            {
                return id.Split('/')[1].Trim();
            }

            return id;
        }

        static public string FolderName(this string id)
        {
            if (id.Contains("@"))
            {
                return id.Split('@')[0].Trim();
            }
            else if (id.Contains("/"))
            {
                return id.Split('/')[0].Trim();
            }

            return String.Empty;
        }

        static public string CombineUri(this string baseUrl, string urlPath)
        {
            while (baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);
            }

            while (urlPath.StartsWith("/"))
            {
                urlPath = urlPath.Substring(1);
            }

            return baseUrl + "/" + urlPath;
            //return new Uri(new Uri(baseUrl), urlPath).ToString();
        }

        static public string ToValidUri(this string uri)
        {
            int pos1 = uri.IndexOf("://") < 0 ? 0 : uri.IndexOf("://") + 3;
            int pos2 = uri.IndexOf("?") < 0 ? uri.Length : uri.IndexOf("?"), pos;

            while ((pos = uri.LastIndexOf("//")) > 0)
            {
                if (pos > pos1 && pos < pos2)
                {
                    uri = uri.Substring(0, pos) + uri.Substring(pos + 1);
                    pos2 = pos2--;
                }
                else
                {
                    break;
                }
            }

            return uri;
        }

        static public IEnvelope FullExtent(this IServiceMap map)
        {
            Envelope fullExtent = null;
            var displaySRef = map?.Display?.SpatialReference;

            if (map?.MapElements != null)
            {
                foreach (var layer in map.MapElements)
                {
                    IEnvelope envelope = null;
                    ISpatialReference layerSRef = null;

                    if (layer.Class is IFeatureClass featureClass && featureClass.Envelope != null)
                    {
                        envelope = featureClass.Envelope;
                        layerSRef = featureClass.SpatialReference;
                    }
                    else if (layer.Class is IRasterClass rasterClass && rasterClass.Polygon != null)
                    {
                        envelope = rasterClass.Polygon.Envelope;
                        layerSRef = rasterClass.SpatialReference;
                    }

                    if (envelope == null)
                    {
                        continue;
                    }

                    // A layer's envelope is stored in its own (native) spatial reference, which
                    // can differ from the map's display spatial reference (eg. Web Mercator display
                    // over natively projected source data). Without reprojecting here, the returned
                    // extent ends up with coordinate values from the native SRef mislabeled as being
                    // in the display SRef - which then misleads clients (ArcGIS REST/GeoJSON capabilities)
                    // relying on this extent for a spatial reference it doesn't actually match.
                    if (displaySRef != null && layerSRef != null && !layerSRef.Equals(displaySRef))
                    {
                        envelope = GeometricTransformerFactory
                            .Transform2D(envelope, layerSRef, displaySRef, map.Display?.DatumTransformations)?
                            .Envelope ?? envelope;
                    }

                    if (fullExtent == null)
                    {
                        fullExtent = new Framework.Geometry.Envelope(envelope);
                    }
                    else
                    {
                        fullExtent.Union(envelope);
                    }
                }
            }

            return fullExtent;
        }

        static public void ToConsole(this Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Exception      : {ex.GetType().ToString()}");
                Console.WriteLine($"     Message   : {ex.Message}");
                Console.WriteLine($"     Stacktrace: {ex.StackTrace}");
            }
        }
    }
}
