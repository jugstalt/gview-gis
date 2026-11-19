using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using System.Security.Claims;

namespace gView.Server.Tests;

public class AuthTokenExtensionsTests
{
    [Fact]
    public void ClaimsPrincipal_RoundTrip_KeepsUserTypeExpireAndStamp()
    {
        var token = new AuthToken("admin", AuthToken.AuthTypes.Manage, TimeSpan.FromHours(8)) { Stamp = "abc" };

        var result = token.ToClaimsPrincipal().ToAuthToken();

        Assert.Equal("admin", result.Username);
        Assert.Equal(AuthToken.AuthTypes.Manage, result.AuthType);
        Assert.Equal(token.Expire, result.Expire);
        Assert.Equal("abc", result.Stamp);
    }

    [Fact]
    public void ClaimsPrincipal_NameIdentifier_DoesNotChangeOnRenewal()
    {
        // antiforgery tokens are bound to the NameIdentifier: a renewed session must keep it
        var first = new AuthToken("admin", AuthToken.AuthTypes.Manage, TimeSpan.FromHours(1)) { Stamp = "abc" };
        var renewed = new AuthToken("admin", AuthToken.AuthTypes.Manage, TimeSpan.FromHours(8)) { Stamp = "abc" };

        Assert.Equal(
            first.ToClaimsPrincipal().FindFirst(ClaimTypes.NameIdentifier)!.Value,
            renewed.ToClaimsPrincipal().FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
