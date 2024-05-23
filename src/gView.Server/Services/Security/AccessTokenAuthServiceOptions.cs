namespace gView.Server.Services.Security
{
    public class AccessTokenAuthServiceOptions
    {
        public string Authority { get; set; }
        public string AccessTokenParameterName { get; set; }
        public bool AllowAccessTokenAuthorization { get; set; }
    }
}
