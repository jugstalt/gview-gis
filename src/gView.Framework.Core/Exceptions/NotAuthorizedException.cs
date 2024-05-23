using System;

namespace gView.Framework.Core.Exceptions
{
    public class NotAuthorizedException : MapServerAuthException
    {
        public NotAuthorizedException() : base("Not Authorized") { }
        public NotAuthorizedException(string message) : base(message) { }
    }
}
