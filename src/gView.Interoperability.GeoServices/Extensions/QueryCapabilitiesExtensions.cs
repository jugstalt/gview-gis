using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using gView.Interoperability.GeoServices.Rest.DTOs.Request;
using System;

namespace gView.Interoperability.GeoServices.Extensions
{
    /// <summary>
    /// Rejects query parameters of capabilities that are switched off for the
    /// service or layer (like ArcGIS Server does for unsupported parameters).
    /// </summary>
    static internal class QueryCapabilitiesExtensions
    {
        static public void CheckQueryCapabilities(this JsonQueryLayerDTO query, IMapServiceProperties properties, int layerId)
        {
            if (properties is null)
            {
                return;
            }

            void Require(bool requested, RestCapability capability, string parameters)
            {
                if (requested && !properties.IsCapabilityEnabled(capability, layerId, true))
                {
                    throw new MapServerException($"Layer does not support {parameters}");
                }
            }

            Require(query.IsStatisticsQuery(), RestCapability.Statistics, "outStatistics/groupByFieldsForStatistics");
            Require(query.ResultOffset > 0 || query.ResultRecordCount > 0, RestCapability.Pagination, "resultOffset/resultRecordCount");
            Require(!String.IsNullOrWhiteSpace(query.OrderByFields), RestCapability.OrderBy, "orderByFields");
            Require(query.ReturnDistinctValues, RestCapability.Distinct, "returnDistinctValues");
            Require(query.ReturnExtentOnly, RestCapability.ReturningQueryExtent, "returnExtentOnly");
        }
    }
}
