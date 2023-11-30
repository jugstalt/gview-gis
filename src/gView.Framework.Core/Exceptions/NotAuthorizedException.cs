using System;

namespace gView.Framework.Core.Exceptions
{
    public class NotAuthorizedException : MapServerException
    {
        public NotAuthorizedException() : base("Not Authorized") { }
        public NotAuthorizedException(string message) : base(message) { }
    }
}
