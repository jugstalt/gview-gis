using System;

namespace gView.Framework.Core.Exceptions
{
    public class InvalidTokenException : MapServerException
    {
        public InvalidTokenException() : base("Invalid Token (498)") { }
    }
}
