namespace gView.Framework.Core.Carto
{
    /// <summary>
    /// Tri-state switch for a capability of a map service or layer.
    /// <see cref="Auto"/> keeps the behavior that is derived from the data source.
    /// </summary>
    public enum CapabilitySwitch
    {
        Auto = 0,
        On = 1,
        Off = 2
    }

    /// <summary>
    /// Capabilities of the GeoServices REST (ArcGIS Server) interface that can be switched
    /// per service and per layer. A layer setting overrides the service setting.
    /// </summary>
    public enum RestCapability
    {
        /// <summary>outStatistics, groupByFieldsForStatistics (layer)</summary>
        Statistics = 0,
        /// <summary>resultOffset, resultRecordCount (layer)</summary>
        Pagination = 1,
        /// <summary>orderByFields (layer)</summary>
        OrderBy = 2,
        /// <summary>returnDistinctValues (layer)</summary>
        Distinct = 3,
        /// <summary>returnExtentOnly (layer)</summary>
        ReturningQueryExtent = 4,
        /// <summary>dynamic legends (layer)</summary>
        DynamicLegends = 5,
        /// <summary>dynamicLayers parameter of export (service only)</summary>
        DynamicLayers = 6
    }
}
