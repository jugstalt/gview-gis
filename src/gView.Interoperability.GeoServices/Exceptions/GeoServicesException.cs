using System;

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
