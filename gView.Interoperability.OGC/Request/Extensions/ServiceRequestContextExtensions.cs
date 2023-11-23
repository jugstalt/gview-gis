using gView.MapServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.OGC.Request.Extensions
{
    static internal class ServiceRequestContextExtensions
    {
        static public string ToLayerName(this IServiceRequestContext context, string layerId)
            => string.Format(
            context.GetContextMetadata(
                WMSRequest.LayerNameFormatMetadataKey,
                WMSRequest.LayerNameFormatDefault), layerId);
        
        static public string ToLayerId(this IServiceRequestContext context, string layerName)
        {
            string format = context.GetContextMetadata(
                WMSRequest.LayerNameFormatMetadataKey,
                WMSRequest.LayerNameFormatDefault);

            // reverse...
            return format switch
            {
                "{0}" => layerName,
                "c{0}" => layerName.Substring(1),
                _ => throw new Exception("Can't determine layerid from name")
            };
        }
    }
}
