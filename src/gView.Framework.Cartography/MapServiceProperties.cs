using gView.Framework.Core.Carto;
using gView.Framework.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Cartography
{
    public class MapServiceProperties : IMapServiceProperties
    {
        private const string CapabilityKeyPrefix = "MapService_Cap_";
        private const string LayerCapabilitiesKey = "MapService_LayerCaps";

        // only non-Auto values are stored
        private Dictionary<RestCapability, CapabilitySwitch> _capabilities = new();
        private Dictionary<int, Dictionary<RestCapability, CapabilitySwitch>> _layerCapabilities = new();

        public int MaxImageWidth { get; set; }
        
        public int MaxImageHeight { get; set; }

        public int MaxRecordCount { get; set; }

        public int MaxStatisticsRecordScan { get; set; }

        #region Capabilities

        public CapabilitySwitch GetCapability(RestCapability capability)
            => _capabilities.TryGetValue(capability, out var value) ? value : CapabilitySwitch.Auto;

        public void SetCapability(RestCapability capability, CapabilitySwitch value)
            => Set(_capabilities, capability, value);

        public CapabilitySwitch GetLayerCapability(int layerId, RestCapability capability)
            => _layerCapabilities.TryGetValue(layerId, out var layer) && layer.TryGetValue(capability, out var value)
                ? value
                : CapabilitySwitch.Auto;

        public void SetLayerCapability(int layerId, RestCapability capability, CapabilitySwitch value)
        {
            if (!_layerCapabilities.TryGetValue(layerId, out var layer))
            {
                if (value == CapabilitySwitch.Auto)
                {
                    return;
                }

                _layerCapabilities[layerId] = layer = new();
            }

            Set(layer, capability, value);

            if (layer.Count == 0)
            {
                _layerCapabilities.Remove(layerId);
            }
        }

        public bool IsCapabilityEnabled(RestCapability capability, int? layerId, bool autoValue)
        {
            var value = layerId.HasValue ? GetLayerCapability(layerId.Value, capability) : CapabilitySwitch.Auto;
            if (value == CapabilitySwitch.Auto)
            {
                value = GetCapability(capability);
            }

            return value switch
            {
                CapabilitySwitch.On => true,
                CapabilitySwitch.Off => false,
                _ => autoValue
            };
        }

        /// <summary>Copies all capability switches (service and layers) from another instance.</summary>
        public void CopyCapabilitiesFrom(MapServiceProperties other)
        {
            _capabilities = new(other._capabilities);
            _layerCapabilities = other._layerCapabilities
                .ToDictionary(e => e.Key, e => new Dictionary<RestCapability, CapabilitySwitch>(e.Value));
        }

        private static void Set(Dictionary<RestCapability, CapabilitySwitch> dict, RestCapability capability, CapabilitySwitch value)
        {
            if (value == CapabilitySwitch.Auto)
            {
                dict.Remove(capability);
            }
            else
            {
                dict[capability] = value;
            }
        }

        #endregion

        public IMapServiceProperties Clone()
        {
            var clone = new MapServiceProperties()
            {
                MaxImageWidth = MaxImageWidth,
                MaxImageHeight = MaxImageHeight,
                MaxRecordCount = MaxRecordCount,
                MaxStatisticsRecordScan = MaxStatisticsRecordScan
            };
            clone.CopyCapabilitiesFrom(this);

            return clone;
        }

        public void Load(IPersistStream stream)
        {
            MaxImageWidth = (int)stream.Load("MapService_MaxImageWidth", (int)0);
            MaxImageHeight = (int)stream.Load("MapService_MaxImageHeight", (int)0);
            MaxRecordCount = (int)stream.Load("MapService_MaxRecordCount", (int)0);
            MaxStatisticsRecordScan = (int)stream.Load("MapService_MaxStatisticsRecordScan", (int)0);

            _capabilities.Clear();
            _layerCapabilities.Clear();

            foreach (RestCapability capability in Enum.GetValues(typeof(RestCapability)))
            {
                SetCapability(capability, (CapabilitySwitch)(int)stream.Load($"{CapabilityKeyPrefix}{capability}", (int)0));
            }

            LayerCapabilitiesPersist layerPersist;
            while ((layerPersist = (LayerCapabilitiesPersist)stream.Load(LayerCapabilitiesKey, null, new LayerCapabilitiesPersist())) != null)
            {
                foreach (var capability in layerPersist.Values)
                {
                    SetLayerCapability(layerPersist.LayerId, capability.Key, capability.Value);
                }
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("MapService_MaxImageWidth", MaxImageWidth);
            stream.Save("MapService_MaxImageHeight", MaxImageHeight);
            stream.Save("MapService_MaxRecordCount", MaxRecordCount);
            stream.Save("MapService_MaxStatisticsRecordScan", MaxStatisticsRecordScan);

            foreach (var capability in _capabilities)
            {
                stream.Save($"{CapabilityKeyPrefix}{capability.Key}", (int)capability.Value);
            }

            foreach (var layer in _layerCapabilities.OrderBy(l => l.Key))
            {
                stream.Save(LayerCapabilitiesKey, new LayerCapabilitiesPersist()
                {
                    LayerId = layer.Key,
                    Values = layer.Value
                });
            }
        }

        #region Classes

        private class LayerCapabilitiesPersist : IPersistable
        {
            public int LayerId { get; set; }
            public Dictionary<RestCapability, CapabilitySwitch> Values { get; set; } = new();

            public void Load(IPersistStream stream)
            {
                LayerId = (int)stream.Load("LayerId", (int)0);

                Values = new();
                foreach (RestCapability capability in Enum.GetValues(typeof(RestCapability)))
                {
                    var value = (CapabilitySwitch)(int)stream.Load(capability.ToString(), (int)0);
                    if (value != CapabilitySwitch.Auto)
                    {
                        Values[capability] = value;
                    }
                }
            }

            public void Save(IPersistStream stream)
            {
                stream.Save("LayerId", LayerId);
                foreach (var capability in Values)
                {
                    stream.Save(capability.Key.ToString(), (int)capability.Value);
                }
            }
        }

        #endregion
    }
}
