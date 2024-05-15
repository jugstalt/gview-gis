namespace gView.Framework.Core.Exceptions
{
    public class TokenRequiredException : MapServerAuthException
    {
        public TokenRequiredException() : base("Token required (499)") { }
        public TokenRequiredException(string message) : base($"Token required (499): {message}") { }
    }
}
