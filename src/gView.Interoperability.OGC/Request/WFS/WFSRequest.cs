#nullable enable

using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Extensions;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.OGC.GML;
using gView.Framework.OGC.WFS;
using gView.Interoperability.Extensions.Features;
using gView.Interoperability.OGC.Request.WFS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Interoperability.OGC
{
    /// <summary>
    /// OGC Web Feature Service 2.0.0, 1.1.0 and 1.0.0 (read only). The feature types are the
    /// feature collections of the OGC API (gv:c{layerId}), the features are read with the same
    /// <see cref="FeatureReader"/>.
    /// </summary>
    [RegisterPlugIn("391FA941-3E31-456e-8A3A-703E07962BA6")]
    public class WFSRequest : IServiceRequestInterpreter
    {
        private IMapServer? _mapServer = null;

        #region IServiceRequestInterpreter Member

        public void OnCreate(IMapServer mapServer)
        {
            _mapServer = mapServer;
        }

        async public Task Request(IServiceRequestContext context)
        {
            if (context?.ServiceRequest == null || _mapServer == null)
            {
                return;
            }

            var serviceRequest = context.ServiceRequest;
            var version = GuessVersion(serviceRequest.Request);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var parameters = WfsRequestParameters.Parse(serviceRequest.Request);
                string operation = parameters.Request;

                if (operation.Equals("GetCapabilities", StringComparison.OrdinalIgnoreCase))
                {
                    version = NegotiateVersion(parameters);
                    await GetCapabilities(context, parameters, version);
                }
                else
                {
                    version = RequestVersion(parameters);

                    switch (operation.ToLowerInvariant())
                    {
                        case "describefeaturetype":
                            await DescribeFeatureType(context, parameters, version);
                            break;
                        case "getfeature":
                            await GetFeature(context, parameters, version);
                            break;
                        case "transaction":
                        case "lockfeature":
                        case "getfeaturewithlock":
                        case "getgmlobject":
                        case "getpropertyvalue":
                        case "liststoredqueries":
                        case "describestoredqueries":
                        case "createstoredquery":
                        case "dropstoredquery":
                            throw WfsException.OperationNotSupported(operation);
                        default:
                            throw WfsException.InvalidParameterValue("request", $"Unknown request {operation}");
                    }
                }

                await _mapServer.LogAsync(context, "Service:" + serviceRequest.Service, loggingMethod.request,
                    $"WFS {version.VersionString} {operation} ({stopwatch.ElapsedMilliseconds} ms)");
            }
            catch (WfsException ex)
            {
                WriteException(serviceRequest, version, ex.Code, ex.Message, ex.Locator, ex.HttpStatusCode);

                await _mapServer.LogAsync(context, "Service:" + serviceRequest.Service, loggingMethod.request,
                    $"WFS {version.VersionString} {ex.Code}: {ex.Message}");
            }
            catch (Exception ex)
            {
                string errorId = serviceRequest.ErrorId;

                await _mapServer.LogAsync(context, "Service:" + serviceRequest.Service, loggingMethod.error,
                    ex.ToLogMessage(errorId));

                WriteException(serviceRequest, version, "NoApplicableCode", ex.ToClientMessage(errorId), null, 500);
            }
        }

        public AccessTypes RequiredAccessTypes(IServiceRequestContext context) => AccessTypes.Query;

        public string IdentityName => "wfs";
        public string IdentityLongName => "OGC Web Feature Service";

        public int Priority => 50;

        public InterpreterCapabilities Capabilities
            => new InterpreterCapabilities(new InterpreterCapabilities.Capability[]{
                    new InterpreterCapabilities.LinkCapability("GetCapabilities","{server}/ogc/{folder@service}?VERSION=2.0.0&SERVICE=WFS&REQUEST=GetCapabilities","2.0.0") { IsPrimary = true },
                    new InterpreterCapabilities.LinkCapability("GetCapabilities","{server}/ogc/{folder@service}?VERSION=1.1.0&SERVICE=WFS&REQUEST=GetCapabilities","1.1.0"),
                    new InterpreterCapabilities.LinkCapability("GetCapabilities","{server}/ogc/{folder@service}?VERSION=1.0.0&SERVICE=WFS&REQUEST=GetCapabilities","1.0.0")
                });

        #endregion

        #region Operations

        async private Task GetCapabilities(IServiceRequestContext context, WfsRequestParameters parameters, WfsVersion version)
        {
            using var map = await CreateServiceMap(context);

            var writer = new WfsCapabilitiesWriter(map, new WfsFeatureTypes(map), OnlineResource(context.ServiceRequest), version);

            context.ServiceRequest.Response = writer.Write();
            context.ServiceRequest.ResponseContentType = "text/xml";
        }

        async private Task DescribeFeatureType(IServiceRequestContext context, WfsRequestParameters parameters, WfsVersion version)
        {
            using var map = await CreateServiceMap(context);

            var collections = new WfsFeatureTypes(map).Resolve(parameters.TypeNames, version == WfsVersion.V200 ? "typeNames" : "typeName", allIfEmpty: true);

            GmlFormat format = "XMLSCHEMA".Equals(parameters.OutputFormat, StringComparison.OrdinalIgnoreCase)
                ? GmlFormat.Gml2
                : WfsVersions.ParseGmlOutputFormat(parameters.OutputFormat, version)
                  ?? throw WfsException.InvalidParameterValue("outputFormat", $"Unsupported outputFormat {parameters.OutputFormat}");

            context.ServiceRequest.Response = WfsSchemaWriter.Write(collections, format);
            context.ServiceRequest.ResponseContentType = WfsVersions.ContentType(format);
        }

        async private Task GetFeature(IServiceRequestContext context, WfsRequestParameters parameters, WfsVersion version)
        {
            using var map = await CreateServiceMap(context);
            var featureTypes = new WfsFeatureTypes(map);

            string typeNamesLocator = version == WfsVersion.V200 ? "typeNames" : "typeName";

            var typeNames = parameters.TypeNames.Count == 0 && parameters.ResourceIds.Count > 0
                ? parameters.ResourceIds.Select(FesFilterParser.ResourceIdTypeName).OfType<string>().Distinct().ToArray()  // RESOURCEID without TYPENAMES
                : parameters.TypeNames;
            var collections = featureTypes.Resolve(typeNames, typeNamesLocator, allIfEmpty: false);
            if (collections.Count == 0)
            {
                throw WfsException.MissingParameterValue(typeNamesLocator);
            }

            var format = WfsVersions.ParseGmlOutputFormat(parameters.OutputFormat, version)
                ?? throw WfsException.InvalidParameterValue("outputFormat", $"Unsupported outputFormat {parameters.OutputFormat}");

            bool hitsOnly = parameters.ResultType?.ToLowerInvariant() switch
            {
                null or "results" => false,
                "hits" when version != WfsVersion.V100 => true,
                _ => throw WfsException.InvalidParameterValue("resultType", $"Unsupported resultType {parameters.ResultType}")
            };

            var outCrs = parameters.SrsName is not null
                ? ParseCrs(parameters.SrsName, "srsName")
                : WfsFeatureTypes.DefaultCrs(map, collections[0], version);

            #region Count

            int count = parameters.Count ?? 0;
            int maxRecordCount = map.MapServiceProperties?.MaxRecordCount ?? 0;
            if (maxRecordCount > 0 && (count <= 0 || count > maxRecordCount))
            {
                count = maxRecordCount;
            }

            #endregion

            var query = new WfsQuery(map, parameters, version, outCrs) { Context = context };
            var result = await query.ExecuteAsync(
                collections,
                count,
                countMatched: version == WfsVersion.V200 && NumberMatchedEnabled(map),
                hitsOnly);

            var writer = new WfsFeatureWriter(version, format, outCrs?.Name, version.SwapAxes(outCrs, query.OutSpatialReference));
            context.ServiceRequest.Response = writer.Write(
                result,
                hitsOnly,
                DescribeFeatureTypeUrl(context.ServiceRequest, version, format, collections),
                DateTime.UtcNow,
                next: version == WfsVersion.V200 && result.HasMore && count > 0 && !hitsOnly
                    ? PageUrl(context.ServiceRequest, parameters, parameters.StartIndex + count, count)
                    : null,
                previous: version == WfsVersion.V200 && parameters.StartIndex > 0 && count > 0 && !hitsOnly
                    ? PageUrl(context.ServiceRequest, parameters, Math.Max(0, parameters.StartIndex - count), count)
                    : null);
            context.ServiceRequest.ResponseContentType = WfsVersions.ContentType(format);
        }

        /// <summary>
        /// numberMatched for resultType=results: per service (WFS export metadata) or the server default
        /// </summary>
        private static bool NumberMatchedEnabled(IServiceMap map)
            => (map.MetadataProvider(WFS_Export_Metadata.MetadataGuid) as WFS_Export_Metadata)?.NumberMatched
               ?? WfsOptions.NumberMatchedDefault;

        /// <summary>
        /// The KVP request with another STARTINDEX/COUNT, null for XML requests
        /// </summary>
        internal static string? PageUrl(ServiceRequest serviceRequest, WfsRequestParameters parameters, int startIndex, int count)
        {
            if (parameters.IsXmlRequest)
            {
                return null;
            }

            var items = parameters.KvpParameters
                .Where(p => !p.Key.Equals("STARTINDEX", StringComparison.OrdinalIgnoreCase)
                         && !p.Key.Equals("COUNT", StringComparison.OrdinalIgnoreCase)
                         && !p.Key.Equals("MAXFEATURES", StringComparison.OrdinalIgnoreCase))
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}")
                .Append($"STARTINDEX={startIndex.ToString(CultureInfo.InvariantCulture)}")
                .Append($"COUNT={count.ToString(CultureInfo.InvariantCulture)}");

            return $"{serviceRequest.OnlineResource}?{String.Join("&", items)}".Replace("??", "?");
        }

        #endregion

        #region Helper

        async private Task<IServiceMap> CreateServiceMap(IServiceRequestContext context)
            => await context.CreateServiceMapInstance()
               ?? throw new Exception($"Service {context.ServiceRequest.Service} not found");

        internal static CrsReference ParseCrs(string value, string locator)
        {
            if (!CrsReference.TryParse(value, out var crs) || !IsKnownCrs(crs))
            {
                throw WfsException.InvalidParameterValue(locator, $"Unknown or unsupported CRS {value}");
            }
            return crs;
        }

        private static bool IsKnownCrs(CrsReference crs)
        {
            try
            {
                // unknown EPSG codes give a spatial reference without projection parameters
                return crs.ToSpatialReference() is { } sref
                       && sref.Parameters?.Any(p => p.StartsWith("+proj=", StringComparison.OrdinalIgnoreCase)) == true;
            }
            catch
            {
                return false;  // unknown EPSG code
            }
        }

        private static string OnlineResource(ServiceRequest serviceRequest)
            => (serviceRequest.OnlineResource + "?SERVICE=WFS&").Replace("??", "?");

        private static string DescribeFeatureTypeUrl(ServiceRequest serviceRequest, WfsVersion version, GmlFormat format, IEnumerable<OgcCollectionLayer> collections)
        {
            string url = $"{OnlineResource(serviceRequest)}VERSION={version.VersionString}&REQUEST=DescribeFeatureType&{(version == WfsVersion.V200 ? "TYPENAMES" : "TYPENAME")}="
                + Uri.EscapeDataString(String.Join(",", collections.Select(c => WfsFeatureTypes.QualifiedName(c))));

            if (format != version.DefaultGmlFormat)
            {
                url += "&OUTPUTFORMAT=" + Uri.EscapeDataString(version.OutputFormatName(format));
            }

            return url;
        }

        /// <summary>
        /// GetCapabilities: the first supported of AcceptVersions, otherwise VERSION or the newest version
        /// </summary>
        internal static WfsVersion NegotiateVersion(WfsRequestParameters parameters)
        {
            if (parameters.AcceptVersions.Count > 0)
            {
                foreach (var accepted in parameters.AcceptVersions)
                {
                    var version = WfsVersions.Parse(accepted);
                    if (version.HasValue)
                    {
                        return version.Value;
                    }
                }
                throw new WfsException("VersionNegotiationFailed",
                    $"None of the versions {String.Join(",", parameters.AcceptVersions)} is supported", "acceptVersions");
            }

            return WfsVersions.Parse(parameters.VersionString) ?? WfsVersions.Supported[0];
        }

        internal static WfsVersion RequestVersion(WfsRequestParameters parameters)
        {
            if (String.IsNullOrWhiteSpace(parameters.VersionString))
            {
                throw WfsException.MissingParameterValue("version");
            }

            return WfsVersions.Parse(parameters.VersionString)
                ?? throw WfsException.InvalidParameterValue("version", $"Unsupported version {parameters.VersionString}");
        }

        /// <summary>
        /// The version for exception reports of requests that can't be parsed
        /// </summary>
        private static WfsVersion GuessVersion(string request)
        {
            try
            {
                string? versionString = request.TrimStart().StartsWith("<")
                    ? System.Text.RegularExpressions.Regex.Match(request, "version\\s*=\\s*[\"']([^\"']+)[\"']").Groups[1].Value
                    : WfsRequestParameters.ParseQueryString(request).GetValueOrDefault("VERSION");

                return WfsVersions.Parse(versionString) ?? WfsVersion.V200;
            }
            catch
            {
                return WfsVersion.V200;
            }
        }

        private static void WriteException(ServiceRequest serviceRequest, WfsVersion version, string code, string message, string? locator, int httpStatusCode)
        {
            serviceRequest.Succeeded = false;
            serviceRequest.Response = WfsExceptionReport.Write(version, code, message, locator);
            serviceRequest.ResponseContentType = "text/xml";
            serviceRequest.ResponseStatusCode = version == WfsVersion.V200 ? httpStatusCode : null;
        }

        #endregion
    }
}
