using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Core.Framework.Exceptions
{
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException() : base() { }
        public NotAuthorizedException(string message) : base(message) { }
    }

    public class TokenRequiredException : Exception
    {
        public TokenRequiredException() : base("Token required (499)") { }
        public TokenRequiredException(string message) : base("Token required (499): " + message) { }
    }

    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base("Invalid Token (498)") { }
    }

    public class MapServerException : Exception
    {
        public MapServerException(string messsage)
            : base(messsage)
        {

        }
    }
}
