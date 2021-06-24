using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Exceptions
{
    public class GeoServicesException : Exception
    {
        public GeoServicesException() { }
        public GeoServicesException(string message, int errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public int ErrorCode { get; }
    }
}
