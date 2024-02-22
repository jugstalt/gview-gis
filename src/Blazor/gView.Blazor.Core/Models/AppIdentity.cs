namespace gView.Blazor.Core.Models;

public class AppIdentity
{
    public AppIdentity(
            string username, 
            bool isAdministrator,
            bool isAuthorizedUser,
            bool canLogout = true
        )
    {
        this.Username = username;
        this.IsAdministrator = isAdministrator;
        this.IsAuthorizedUser = isAuthorizedUser;
        this.CanLogout = canLogout;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Username);

    public string Username { get; }

    public bool IsAdministrator { get; }
    
    public bool IsAuthorizedUser { get; }

    public bool CanLogout { get; }
}
