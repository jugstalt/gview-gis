using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace gView.Framework.OGC.WMS
{
    public class CapabilitiesHelper
    {
        public static NumberFormatInfo Nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        private object _caps = null;
        private WMSLayers _layers = new WMSLayers();
        private string[] _imageFormats = null, _getFeatureInfoFormats = null;
        private string _getMapOnlineResource = String.Empty, _getFeatureInfoOnlineResource = String.Empty, _getLegendGraphicOnlineResource = String.Empty;

        public CapabilitiesHelper(object capabilities)
        {
            _caps = capabilities;

            if (_caps is Version_1_1_1.WMT_MS_Capabilities)
            {
                Version_1_1_1.WMT_MS_Capabilities caps = (Version_1_1_1.WMT_MS_Capabilities)_caps;

                #region GetMap
                if (caps.Capability.Request.GetMap != null &&
                    caps.Capability.Request.GetMap.DCPType != null &&
                    caps.Capability.Request.GetMap.DCPType.Length > 0)
                {
                    foreach (object http in caps.Capability.Request.GetMap.DCPType[0].HTTP)
                    {
                        if (http is Version_1_1_1.Get)
                            _getMapOnlineResource = ((Version_1_1_1.Get)http).OnlineResource.href;
                    }
                }
                if (caps.Capability.Request.GetMap != null)
                    _imageFormats = caps.Capability.Request.GetMap.Format;
                #endregion

                #region GetFeatureInfo
                if (caps.Capability.Request.GetFeatureInfo != null &&
                    caps.Capability.Request.GetFeatureInfo.DCPType != null &&
                    caps.Capability.Request.GetFeatureInfo.DCPType.Length > 0)
                {
                    foreach (object http in caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP)
                    {
                        if (http is Version_1_1_1.Get)
                            _getFeatureInfoOnlineResource = ((Version_1_1_1.Get)http).OnlineResource.href;
                    }
                }
                if (caps.Capability.Request.GetFeatureInfo != null)
                    _getFeatureInfoFormats = caps.Capability.Request.GetFeatureInfo.Format;
                #endregion

                #region GetLegendGraphic
                if (caps.Capability.Request.GetLegendGraphic != null &&
                    caps.Capability.Request.GetLegendGraphic.DCPType != null &&
                    caps.Capability.Request.GetLegendGraphic.DCPType.Length > 0)
                {
                    foreach (object http in caps.Capability.Request.GetLegendGraphic.DCPType[0].HTTP)
                    {
                        if (http is Version_1_1_1.Get)
                            _getLegendGraphicOnlineResource = ((Version_1_1_1.Get)http).OnlineResource.href;
                    }
                }
                #endregion

                #region Layers
                for (int i = 0; i < caps.Capability.Layer.Layer1.Length; i++)
                {
                    Version_1_1_1.Layer layer = caps.Capability.Layer.Layer1[i];

                    WMSLayer wmslayer = new WMSLayer(layer.Name, layer.Title);
                    if (layer.Style != null && layer.Style.Length > 0)
                    {
                        for (int s = 0; s < layer.Style.Length; s++)
                            wmslayer.Styles.Add(new WMSStyle(layer.Style[s].Name, layer.Style[s].Title));
                    }
                    
                    if (layer.ScaleHint != null)
                    {
                        try { wmslayer.MinScale = double.Parse(layer.ScaleHint.min.Replace(",", "."), Nhi) * 2004.4; }
                        catch { }
                        try { wmslayer.MaxScale = double.Parse(layer.ScaleHint.max.Replace(",", "."), Nhi) * 2004.4; }
                        catch { }
                    }
                    _layers.Add(wmslayer);
                }
                #endregion
            }
            else if (_caps is Version_1_3_0.WMS_Capabilities)
            {
                Version_1_3_0.WMS_Capabilities caps = (Version_1_3_0.WMS_Capabilities)_caps;

                #region GetMap
                if (caps.Capability.Request.GetMap.DCPType.Length > 0)
                    _getMapOnlineResource = caps.Capability.Request.GetMap.DCPType[0].HTTP.Get.OnlineResource.href;

                _imageFormats = caps.Capability.Request.GetMap.Format;
                #endregion

                #region GetFeatureInfo
                if (caps.Capability.Request.GetMap.DCPType.Length > 0)
                    _getFeatureInfoOnlineResource = caps.Capability.Request.GetFeatureInfo.DCPType[0].HTTP.Get.OnlineResource.href;

                _getFeatureInfoFormats = caps.Capability.Request.GetFeatureInfo.Format;
                #endregion

                #region Layers
                for (int l = 0; l < caps.Capability.Layer.Length; l++)
                {
                    AddCascadingLayers(caps.Capability.Layer[l], String.Empty);
                }
                #endregion
            }
        }

        private void AddCascadingLayers(Version_1_3_0.Layer layer, string parentName)
        {
            if (layer == null) return;

            if (layer.Layer1 != null)
            {
                parentName = String.IsNullOrEmpty(parentName) ? layer.Title : parentName + "/" + layer.Title;
                for (int i = 0; i < layer.Layer1.Length; i++)
                    AddCascadingLayers(layer.Layer1[i], parentName);
            }
            else
            {
                string title = String.IsNullOrEmpty(parentName) ? layer.Title : parentName + "/" + layer.Title;
                WMSLayer wmslayer = new WMSLayer(layer.Name, title);
                if (layer.Style != null && layer.Style.Length > 0)
                {
                    for (int s = 0; s < layer.Style.Length; s++)
                        wmslayer.Styles.Add(new WMSStyle(layer.Style[s].Name, layer.Style[s].Title));
                }
                if (layer.MinScaleDenominator > 0.0)
                    wmslayer.MinScale = layer.MinScaleDenominator;
                if (layer.MaxScaleDenominator > 0.0)
                    wmslayer.MaxScale = layer.MaxScaleDenominator;

                _layers.Add(wmslayer);
            }
        }

        public WMSLayers Layers
        {
            get { return _layers; }
        }

        public WMSLayers LayersWithStyle
        {
            get
            {
                WMSLayers layers = new WMSLayers();

                foreach (WMSLayer layer in _layers)
                {
                    if (layer.Styles.Count == 0)
                    {
                        layers.Add(layer);
                    }
                    else
                    {
                        foreach (WMSStyle style in layer.Styles)
                        {
                            layers.Add(new WMSLayer(layer.Name + "|" + style.Name, layer.Title + " (" + style.Title + ")"));
                        }
                    }
                }

                return layers;
            }
        }

        public WMSLayer LayerByName(string name)
        {
            string oName = name.Split('|')[0];

            foreach (WMSLayer layer in _layers)
            {
                if (layer.Name == oName)
                    return layer;
            }
            return null;
        }

        public WMSStyle LayerStyleByName(string name)
        {
            string[] p = name.Split('|');
            if (p.Length != 2)
                return null;

            WMSLayer layer = LayerByName(name);
            if (layer == null)
                return null;

            foreach (WMSStyle style in layer.Styles)
                if (style.Name == p[1])
                    return style;

            return null;
        }

        public string[] ImageFormats
        {
            get { return _imageFormats; }
        }
        public string GetMapOnlineResouce
        {
            get { return _getMapOnlineResource; }
        }
        public string[] GetFeatureInfoFormats
        {
            get { return _getFeatureInfoFormats; }
        }
        public string GetFeatureInfoOnlineResouce
        {
            get { return _getFeatureInfoOnlineResource; }
        }
        public string GetLegendGraphicOnlineResource
        {
            get { return _getLegendGraphicOnlineResource; }
        }
        #region Classes
        public class WMSLayers : List<WMSLayer>
        {

        }

        public class WMSLayer
        {
            public string Name = String.Empty;
            public string Title = String.Empty;
            public List<WMSStyle> Styles = new List<WMSStyle>();
            public double MinScale = 0.0;
            public double MaxScale = 0.0;

            public WMSLayer(string name, string title)
            {
                Name = name;
                Title = title;
            }
        }

        public class WMSStyle
        {
            public string Name = String.Empty;
            public string Title = String.Empty;

            public WMSStyle(string name, string title)
            {
                Name = name;
                Title = title;
            }
        }
        #endregion
    }
}
