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

            string typeNamesLocator = version == WfsVersion.V200 ? "typeNames" : "typeName";
            var collections = new WfsFeatureTypes(map).Resolve(parameters.TypeNames, typeNamesLocator, allIfEmpty: false);
            if (collections.Count == 0)
            {
                throw WfsException.MissingParameterValue(typeNamesLocator);
            }

            var format = WfsVersions.ParseGmlOutputFormat(parameters.OutputFormat, version)
                ?? throw WfsException.InvalidParameterValue("outputFormat", $"Unsupported outputFormat {parameters.OutputFormat}");

            if (parameters.ResultType is not null && !parameters.ResultType.Equals("results", StringComparison.OrdinalIgnoreCase))
            {
                throw WfsException.OptionNotSupported("resultType", $"resultType {parameters.ResultType} is not supported");
            }
            if (parameters.StartIndex > 0 && collections.Count > 1)
            {
                throw WfsException.OptionNotSupported("startIndex", "startIndex is only supported for a single feature type");
            }

            #region Output CRS

            CrsReference? outCrs;
            if (parameters.SrsName is not null)
            {
                outCrs = ParseCrs(parameters.SrsName, "srsName");
            }
            else
            {
                outCrs = WfsFeatureTypes.DefaultCrs(map, collections[0], version);
            }

            var outSref = outCrs?.ToSpatialReference();
            bool swapAxes = version.SwapAxes(outCrs, outSref);

            #endregion

            #region Count

            int count = parameters.Count ?? 0;
            int maxRecordCount = map.MapServiceProperties?.MaxRecordCount ?? 0;
            if (maxRecordCount > 0 && (count <= 0 || count > maxRecordCount))
            {
                count = maxRecordCount;
            }

            #endregion

            var featureSets = new List<WfsFeatureSet>();
            int returned = 0;

            foreach (var collection in collections)
            {
                if (count > 0 && returned >= count)
                {
                    break;
                }

                var filter = CreateFilter(collection, parameters, version, outCrs, outSref);
                filter.SetUserData("IServiceRequestContext", context);

                var features = await FeatureReader.ReadListAsync(
                    map, collection, filter, outSref,
                    parameters.StartIndex,
                    count > 0 ? count - returned : 0);

                returned += features.Count;
                featureSets.Add(new WfsFeatureSet(collection, features));
            }

            var writer = new WfsFeatureWriter(version, format, outCrs?.Name, swapAxes);
            context.ServiceRequest.Response = writer.Write(
                featureSets,
                DescribeFeatureTypeUrl(context.ServiceRequest, version, format, collections),
                DateTime.UtcNow);
            context.ServiceRequest.ResponseContentType = WfsVersions.ContentType(format);
        }

        #endregion

        #region Filter

        private IQueryFilter CreateFilter(OgcCollectionLayer collection, WfsRequestParameters parameters, WfsVersion version, CrsReference? outCrs, ISpatialReference? outSref)
        {
            var featureClass = collection.FeatureClass!;
            IQueryFilter filter;

            if (parameters.BBox is not null)
            {
                filter = BBoxFilter(parameters.BBox, version, outCrs, outSref);
            }
            else
            {
                try
                {
                    filter = Filter.FromWFS(featureClass, parameters.Filter, version == WfsVersion.V100 ? GmlVersion.v1 : GmlVersion.v3);
                }
                catch (Exception ex)
                {
                    throw WfsException.InvalidParameterValue("filter", $"Invalid filter: {ex.Message}");
                }

                if (filter is ISpatialFilter spatialFilter && spatialFilter.FilterSpatialReference is null)
                {
                    // filter geometries without srsName are in the CRS of the request
                    spatialFilter.FilterSpatialReference = outSref ?? collection.SpatialReference;
                }
            }

            try
            {
                Filter.AppendSortBy(featureClass, filter, parameters.SortBy);
            }
            catch (Exception ex)
            {
                throw WfsException.InvalidParameterValue("sortBy", $"Invalid sortBy: {ex.Message}");
            }

            // The where clause is generated by the filter parser from checked field names and literals
            filter.WhereClause = FeatureReader.CombineWhereClauses(collection.FilterWhereClause, filter.WhereClause);

            return filter;
        }

        /// <summary>
        /// BBOX=minx,miny,maxx,maxy[,crs] in the axis order of the crs (default: the CRS of the request)
        /// </summary>
        internal static ISpatialFilter BBoxFilter(string bbox, WfsVersion version, CrsReference? defaultCrs, ISpatialReference? defaultSref)
        {
            var parts = bbox.Split(',');
            if (parts.Length < 4)
            {
                throw WfsException.InvalidParameterValue("bbox", $"Invalid bbox: {bbox}");
            }

            var values = new double[4];
            for (int i = 0; i < 4; i++)
            {
                if (!Double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                {
                    throw WfsException.InvalidParameterValue("bbox", $"Invalid bbox: {bbox}");
                }
            }

            var crs = defaultCrs;
            var sref = defaultSref;
            if (parts.Length > 4)
            {
                crs = ParseCrs(String.Join(",", parts.Skip(4)).Trim(), "bbox");
                sref = crs.ToSpatialReference();
            }

            var envelope = version.SwapAxes(crs, sref)
                ? new Envelope(values[1], values[0], values[3], values[2])
                : new Envelope(values[0], values[1], values[2], values[3]);

            return new SpatialFilter()
            {
                Geometry = envelope,
                FilterSpatialReference = sref,
                SpatialRelation = spatialRelation.SpatialRelationIntersects
            };
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
