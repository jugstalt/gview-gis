using gView.Server.Services.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace gView.Server.Tests;

/// <summary>Which bearer tokens are gView's: only those are rejected with 498 when they are invalid</summary>
public class JwtAccessTokenServiceTests
{
    private static string Jwt(string issuer, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a test key that is long enough for HMAC-SHA256!!"));
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("name", "user")]),
            Issuer = issuer,
            NotBefore = expires.AddHours(-2),
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        }));
    }

    [Fact]
    public void IsIssuedBy_GViewTokens_AlsoExpiredOrWithAnotherKey()
    {
        Assert.True(JwtAccessTokenService.IsIssuedBy(Jwt("gview-server", DateTime.UtcNow.AddHours(1)), "gview-server"));
        Assert.True(JwtAccessTokenService.IsIssuedBy(Jwt("gview-server", DateTime.UtcNow.AddHours(-1)), "gview-server"));
    }

    [Theory]
    [InlineData("not-a-jwt")]
    [InlineData("")]
    [InlineData("eyJhbGciOiJIUzI1NiJ9.broken.token")]
    public void IsIssuedBy_NoJwt_IsForeign(string token)
        => Assert.False(JwtAccessTokenService.IsIssuedBy(token, "gview-server"));

    [Fact]
    public void IsIssuedBy_OtherIssuer_IsForeign()
        => Assert.False(JwtAccessTokenService.IsIssuedBy(Jwt("https://login.example.com", DateTime.UtcNow.AddHours(1)), "gview-server"));
}
