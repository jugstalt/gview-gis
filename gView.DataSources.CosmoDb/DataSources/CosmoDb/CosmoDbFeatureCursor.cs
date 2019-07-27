using gView.Framework.Data;
using gView.Framework.Geometry;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.CosmoDb
{
    class CosmoDbFeatureCursor : FeatureCursor
    {
        private IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private IDocumentQuery<dynamic> _query = null;

        public CosmoDbFeatureCursor(CosmoDbFeatureClass fc, IQueryFilter filter)
            : base(fc.SpatialReference, filter.FeatureSpatialReference)
        {
            string sql = String.Empty;
            if (filter is ISpatialFilter)
            {
                ISpatialFilter sFilter = (ISpatialFilter)filter;
                var env = sFilter.Geometry.Envelope;

                env.minx = Math.Max(-180, env.minx);
                env.miny = Math.Max(-90, env.miny);
                env.maxx = Math.Min(180, env.maxx);
                env.maxy = Math.Min(90, env.maxy);

                sql = $"SELECT e._shape FROM everything e WHERE e._fc='{fc.Name}' AND ST_WITHIN(e._shape, {{'type':'Polygon', 'coordinates': [[[{env.miny.ToString(_nhi)}, {env.minx.ToString(_nhi)}], [{env.maxy.ToString(_nhi)}, {env.minx.ToString(_nhi)}], [{env.maxy.ToString(_nhi)}, {env.maxx.ToString(_nhi)}], [{env.miny.ToString(_nhi)}, {env.maxx.ToString(_nhi)}], [{env.miny.ToString(_nhi)}, {env.minx.ToString(_nhi)}]]]}})";
            }
            else
            {
                sql = $"SELECT * FROM everything e WHERE e._fc='{fc.Name}'";
            }

            _query =
                fc.CosmoDocumentClient.CreateDocumentQuery(fc.CosmoDocumentCollection.SelfLink, sql)
                   .AsDocumentQuery();
        }

        private List<dynamic> _dynamics = new List<dynamic>();
        private int _pos = 0;
        async public override Task<IFeature> NextFeature()
        {
            if (_query == null)
            {
                return null;
            };

            var random = new Random();

            if (_dynamics.Count > _pos)
            {
                var dyn = (IDictionary<string, object>)_dynamics[_pos++];

                var feature = new Feature();

                foreach (var fieldName in dyn.Keys)
                {
                    if (fieldName == "_shape")
                    {
                        //feature.Shape = new Point(9 + (double)random.Next(1000) / 1000D, 48 + (double)random.Next(1000) / 1000D);
                        var cosmoShape = (JObject)dyn[fieldName];
                        switch (cosmoShape["type"].Value<string>()?.ToLower())
                        {
                            case "point":
                                var coordinates = (JArray)cosmoShape["coordinates"];
                                feature.Shape = new Point(
                                    coordinates[1].Value<double>(),
                                    coordinates[0].Value<double>());
                                break;
                        }
                    }
                    else
                    {
                        feature.Fields.Add(new FieldValue(fieldName, dyn[fieldName]));
                    }
                }

                Transform(feature);
                return feature;
            }

            _dynamics = new List<dynamic>();
            _pos = 0;
            while (_query.HasMoreResults)
            {
                var next = await _query.ExecuteNextAsync();
                //_dynamics = new List<dynamic>();
                _dynamics.AddRange(next);
                if (_dynamics.Count > 10000)
                {
                    break;
                }
                //_pos = 0;
            }

            if (_dynamics.Count > _pos)
            {
                return await NextFeature();
            }

            return null;
        }
    }
}
