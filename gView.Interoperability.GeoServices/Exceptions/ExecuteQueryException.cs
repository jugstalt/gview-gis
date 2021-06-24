using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Exceptions
{
    public class ExecuteQueryException : GeoServicesException
    {
        public ExecuteQueryException()
            : base("Failed to execute query.", 400) { }
    }
}
