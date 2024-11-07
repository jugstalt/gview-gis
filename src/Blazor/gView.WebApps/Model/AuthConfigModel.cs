namespace gView.WebApps.Model;

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

        public FormUser[]? AdminUsers { get; set; } = null;
        public FormUser[]? CartoUsers { get; set; } = null;

        public class FormUser
        {
            public string Username { get; set; } = "";
            public string PasswordHash { get; set; } = "";
            public string[]? Roles { get; set; } = null;
        }
    }

    #endregion
}