using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Common;
using gView.Framework.Web;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using gView.Interoperability.GeoServices.Rest.DTOs.Request;
using gView.Interoperability.GeoServices.Rest.DTOs.Response;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using gView.Framework.Common.Extensions;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Dataset
{
    public class GeoServicesClass : IWebServiceClass, IDisposable
    {
        internal event EventHandler ModifyResponseOuput = null;

        private GeoServicesDataset _dataset;
        private IBitmap _legend = null;
        private GeorefBitmap _image = null;
        private List<IWebServiceTheme> _clonedThemes = null;

        public GeoServicesClass(GeoServicesDataset dataset)
        {
            _dataset = dataset;
            if (_dataset != null)
            {
                this.Name = _dataset._name;
            }
        }

        #region IWebServiceClass Member

        public event AfterMapRequestEventHandler AfterMapRequest = null;

        async public Task<bool> MapRequest(IDisplay display)
        {
            if (_dataset == null)
            {
                return false;
            }

            List<IWebServiceTheme> themes = Themes;
            if (themes == null)
            {
                return false;
            }

            #region Check for visible Layers

            bool visFound = this.Themes.Where(l => l.Visible).Count() > 0;
            if (!visFound)
            {
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
                return true;
            }

            #endregion Check for visible Layers

            var serviceUrl = _dataset.ServiceUrl();

            //IServiceRequestContext context = display.Map as IServiceRequestContext;

            var jsonExportMap = new JsonExportMapDTO();
            if (display?.Envelope != null)
            {
                var env = display.Envelope;
                jsonExportMap.BBox = $"{env.MinX.ToDoubleString()},{env.MinY.ToDoubleString()},{env.MaxX.ToDoubleString()},{env.MaxY.ToDoubleString()}";
            }

            var sRef = display.SpatialReference ?? this.SpatialReference;
            if (sRef != null)
            {
                jsonExportMap.BBoxSRef = sRef.Name.ToLower().Replace("epsg:", "");
            }
            jsonExportMap.Size = $"{display.ImageWidth},{display.ImageHeight}";

            var layerIds = this.Themes
                .Where(l => l.Visible && (l.Class is IWebFeatureClass || l.Class is IWebRasterClass))
                .Select(l =>
                {
                    if (l.Class is IWebFeatureClass)
                    {
                        return ((IWebFeatureClass)l.Class).ID;
                    }

                    if (l.Class is IWebRasterClass)
                    {
                        return ((IWebRasterClass)l.Class).ID;
                    }

                    return String.Empty;
                });

            jsonExportMap.Layers = $"show:{String.Join(",", layerIds)}";

            var urlParameters = SerializeToUrlParameters(jsonExportMap);

            var response = await _dataset.TryPostAsync<JsonExportResponseDTO>(
                serviceUrl
                    .UrlAppendPath("export")
                    .UrlAppendParameters("f=json")
                    , postData: urlParameters);

            bool hasImage = false;
            if (!String.IsNullOrWhiteSpace(response.Href))
            {
                var imageData = await _dataset._http.GetDataAsync(response.Href);

                if (imageData != null)
                {
                    hasImage = true;
                    _image = new GeorefBitmap(Current.Engine.CreateBitmap(new MemoryStream(imageData)));
                    if (response.Extent != null)
                    {
                        _image.Envelope = new Envelope(response.Extent.Xmin, response.Extent.Ymin, response.Extent.Xmax, response.Extent.Ymax);
                    }
                    _image.SpatialReference = sRef;
                }
            }

            if (!hasImage)
            {
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
                return false;
            }

            return true;
        }

        public Task<bool> LegendRequest(IDisplay display)
        {
            return Task.FromResult(true);
        }

        GeorefBitmap IWebServiceClass.Image
        {
            get { return _image; }
        }

        IBitmap IWebServiceClass.Legend
        {
            get { return _legend; }
        }

        public IEnvelope Envelope
        {
            get;
            internal set;
        }

        public List<IWebServiceTheme> Themes
        {
            get
            {
                if (_clonedThemes != null)
                {
                    return _clonedThemes;
                }

                if (_dataset != null)
                {
                    return _dataset._themes;
                }
                return new List<IWebServiceTheme>();
            }
        }

        internal ISpatialReference _sptatialReference;

        public ISpatialReference SpatialReference
        {
            get { return _sptatialReference; }
            set
            {
                _sptatialReference = value;
                if (_dataset != null)
                {
                    _dataset.SetSpatialReference(value);
                }
            }
        }

        #endregion IWebServiceClass Member

        #region IClass Member

        public string Name
        {
            get; set;
        }

        public string Aliasname
        {
            get { return this.Name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion IClass Member

        #region IClone Member

        public object Clone()
        {
            GeoServicesClass clone = new GeoServicesClass(_dataset);
            clone._clonedThemes = new List<IWebServiceTheme>();

            var themes = (_clonedThemes != null) ?
                _clonedThemes :
                (_dataset?._themes ?? new List<IWebServiceTheme>());

            foreach (IWebServiceTheme theme in themes)
            {
                if (theme == null || theme.Class == null)
                {
                    continue;
                }

                clone._clonedThemes.Add(LayerFactory.Create(
                    theme.Class,
                    theme as ILayer, clone) as IWebServiceTheme);
            }
            clone.AfterMapRequest = AfterMapRequest;
            clone.ModifyResponseOuput = ModifyResponseOuput;
            return clone;
        }

        #endregion IClone Member

        private List<XmlNode> _appendedLayers = new List<XmlNode>();

        internal List<XmlNode> AppendedLayers
        {
            get { return _appendedLayers; }
        }

        private Dictionary<string, XmlNode> _layerRenderer = new Dictionary<string, XmlNode>();

        internal Dictionary<string, XmlNode> LayerRenderer
        {
            get { return _layerRenderer; }
        }

        #region Helper

        public string SerializeToUrlParameters(object obj)
        {
            StringBuilder sb = new StringBuilder();

            if (obj != null)
            {
                foreach (var propertyInfo in obj.GetType().GetProperties())
                {
                    var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
                    if (jsonPropertyAttribute == null)
                    {
                        continue;
                    }

                    string val = propertyInfo.GetValue(obj)?.ToString();
                    if (!String.IsNullOrEmpty(val))
                    {
                        if (propertyInfo.PropertyType == typeof(int) && val == "0")
                        {
                            continue;
                        }

                        if (sb.Length > 0)
                        {
                            sb.Append("&");
                        }

                        sb.Append($"{jsonPropertyAttribute.Name}={HttpUtility.UrlEncode(val)}");
                    }
                }
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            if (_image is not null)
            {
                _image.Dispose();
                _image = null;
            }
        }

        #endregion Helper
    }
}