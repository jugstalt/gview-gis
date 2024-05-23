using System;

namespace gView.Framework.Core.Exceptions
{
    public class InvalidTokenException : MapServerAuthException
    {
        public InvalidTokenException() : base("Invalid Token (498)") { }
    }
}
