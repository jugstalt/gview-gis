using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    public class JsonError
    {
        public Error error { get; set; }
        public class Error
        {
            public int code { get; set; }
            public string message { get; set; }
            public object details { get; set; }
        }
    }
}
