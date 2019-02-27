using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonSecurityToken : JsonStopWatch
    {
        public string token { get; set; }
        public long expires { get; set; }
        public bool ssl => true;
    }
}
