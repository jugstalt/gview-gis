using gView.Interoperability.GeoServices.Rest.Json.Features.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Features
{
    public class JsonFeature
    {
        public JsonFeature()
        {
            this.Attributes = new ExpandoObject();
        }

        [JsonProperty("attributes")]
        public ExpandoObject Attributes { get; set; }
        //JsonAttributes attributes { get; set; }
        [JsonProperty("geometry")]
        public JsonGeometry Geometry { get; set; }
    }
}
