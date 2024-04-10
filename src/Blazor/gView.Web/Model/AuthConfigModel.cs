namespace gView.Web.Model;

public class AuthConfigModel
{
    public string Type { get; set; } = "";

    public OidcClass? Oidc { get; set; }
    public FormsClass? Forms { get; set; }  

    #region Classes

    public class OidcClass 
    {
        public string Authority { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string[]? Scopes { get; set; }

        public string RequiredUserRole { get; set; } = "";
        public string RequiredAdminRole { get; set; } = "";
    }

    public class FormsClass
    {
        public const string AdminRole = "gview-admin";
        public const string UserRole = "gview-user";

        public string AdminUser { get; set; } = "";
        public string AdminPassword { get; set; } = "";

        public string User { get; set; } = "";
        public string Password { get; set; } = "";
    }

    #endregion
}

public class DrivesModel
{
    public Dictionary<string, string>? Drives { get; set; }
}
