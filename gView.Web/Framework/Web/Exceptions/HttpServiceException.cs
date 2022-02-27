using System;

namespace gView.Framework.Web.Exceptions
{
    public class HttpServiceException : Exception
    {
        public HttpServiceException(string message, Exception inner = null)
            : base(message, inner)
        { }
    }
}
