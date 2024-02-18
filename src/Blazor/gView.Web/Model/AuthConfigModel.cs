namespace gView.Web.Model;

public class AuthConfigModel
{
    public string Type { get; set; } = "";

    public OidcClass? Oidc { get; set; }

    public string RequiredUserRole { get; set; } = "";
    public string RequiredAdminRole { get; set; } = "";

    #region Classes

    public class OidcClass 
    {
        public string Authority { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string[]? Scopes { get; set; }
    }

    #endregion
}
