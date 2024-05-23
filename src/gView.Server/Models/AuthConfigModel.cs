#nullable enable

namespace gView.Server.Models;

public class AuthConfigModel
{
    public string Type { get; set; } = "";
    public string RequiredManageRole { get; set; } = "";
    public OidcClass? Oidc { get; set; }

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

