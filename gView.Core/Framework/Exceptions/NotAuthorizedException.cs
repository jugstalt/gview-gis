using System;

namespace gView.Core.Framework.Exceptions
{
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException() : base() { }
        public NotAuthorizedException(string message) : base(message) { }
    }
}
