namespace gView.Server.Services.Security;

/// <summary>
/// All security events (failed logins, lockouts, refused admin requests, admin
/// setup, configuration warnings) are logged under one category, so they can be
/// filtered or forwarded (fail2ban, SIEM). Client ip and username are logged,
/// passwords and tokens never.
/// </summary>
public static class SecurityLog
{
    public const string Category = "gView.Server.Security";
}
