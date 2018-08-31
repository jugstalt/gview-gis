using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.system
{
    public class MapServerResponse
    {
        [JsonProperty(PropertyName="data")]
        public byte[] Data { get; set; }

        [JsonProperty(PropertyName="content-type")]
        public string ContentType { get; set; }

        public DateTime? Expires { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        static public MapServerResponse FromString(string json)
        {
            return JsonConvert.DeserializeObject<MapServerResponse>(json);
        }
    }
}
