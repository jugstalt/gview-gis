using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.OGC.GeoJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace gView.DataSources.GeoJson
{
    class GeoJsonSource
    {
        private readonly string _target;
        private IEnvelope _envelope = null;
        private DateTime _lastLoad = new DateTime(0);
        private List<IFeature> _features = null;

        public GeoJsonSource(string target)
        {
            _target = target;
        }

        async private Task LoadAsync()
        {
            _lastLoad = DateTime.Now;

            string geoJsonString = String.Empty;

            if (_target.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || 
               _target.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                using(var httpClient = new HttpClient())
                {
                    var httpResponseMessage = await httpClient.GetAsync(_target);
                    geoJsonString = await httpResponseMessage.Content.ReadAsStringAsync();
                }
            } 
            else
            {
                geoJsonString = System.IO.File.ReadAllText(_target);
            }

            var geoJson = JsonConvert.DeserializeObject<GeoJsonFeatures>(geoJsonString);
            List<IFeature> features = new List<IFeature>();

            foreach(var geoJsonFeature in geoJson.Features)
            {
                var feature = new Feature();

                feature.Shape = geoJsonFeature.ToGeometry();
                IDictionary<string, object> properties = null;

                if (feature.Shape != null)
                {
                    if (_envelope == null)
                    {
                        _envelope = new Envelope(feature.Shape.Envelope);
                    }
                    else
                    {
                        _envelope.Union(feature.Shape.Envelope);
                    }
                }

                try
                {
                    geoJsonFeature.PropertiesToDict();
                    properties = (IDictionary<string, object>)geoJsonFeature.Properties;
                }
                catch { }

                if(properties!=null)
                {
                    foreach(var key in properties.Keys)
                    {
                        feature.Fields.Add(new FieldValue(key, properties[key]));
                    }
                }

                features.Add(feature);
            }

            _features = features;
        }

        async private Task Refresh()
        {
            if ((DateTime.Now - _lastLoad).TotalMinutes >= 5)
            {
                await LoadAsync();
            }
        }

        public IEnvelope Envelope => _envelope ?? new Envelope();

        async public Task<IEnumerable<IFeature>> GetFeatures<T>()
            where T : IGeometry
        {
            await Refresh();

            var geometryInterfaceType = typeof(T);

            List<IFeature> features = _features == null ?
                new List<IFeature>() :
                new List<IFeature>(_features.Where(f => f?.Shape !=null && geometryInterfaceType.IsAssignableFrom(f.Shape.GetType())));

            return features;
        }

        public Task<IEnumerable<IFeature>> GetFeatures(geometryType geometryType)
        {
            switch(geometryType)
            {
                case geometryType.Point:
                    return GetFeatures<IPoint>();
                case geometryType.Polyline:
                    return GetFeatures<IPolyline>();
                case geometryType.Polygon:
                    return GetFeatures<IPolygon>();
                default:
                    throw new Exception($"geometry type { geometryType } not implemented with GeoJsonSource");
            }
        }
    }
}
