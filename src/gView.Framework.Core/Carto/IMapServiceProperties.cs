using gView.Framework.Core.Common;
using gView.Framework.Core.IO;

namespace gView.Framework.Core.Carto
{
    public interface IMapServiceProperties : IClone<IMapServiceProperties>, IPersistable
    {
        public int MaxImageWidth { get; }
        public int MaxImageHeight { get; }
        public int MaxRecordCount { get; }
        /// <summary>Max. number of source records scanned for in-memory statistics queries (0 = default 1,000,000).</summary>
        public int MaxStatisticsRecordScan { get; }

        /// <summary>Service level switch of a REST capability (<see cref="CapabilitySwitch.Auto"/> if not set).</summary>
        public CapabilitySwitch GetCapability(RestCapability capability);

        /// <summary>Layer level switch of a REST capability (<see cref="CapabilitySwitch.Auto"/> if not set).</summary>
        public CapabilitySwitch GetLayerCapability(int layerId, RestCapability capability);

        /// <summary>
        /// Resolves a REST capability: the layer setting overrides the service setting,
        /// if both are <see cref="CapabilitySwitch.Auto"/> <paramref name="autoValue"/> is returned.
        /// </summary>
        public bool IsCapabilityEnabled(RestCapability capability, int? layerId, bool autoValue);
    }
}