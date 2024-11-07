using System;

namespace gView.Server.Exceptions
{
    public class TokenValidationException : Exception
    {
        public TokenValidationException(string message) :
            base($"Token validation: {message}")
        {

        }
    }
}
