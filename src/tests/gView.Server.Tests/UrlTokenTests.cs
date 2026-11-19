using gView.Framework.Core.Exceptions;
using gView.Framework.Security;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Middleware.Authentication;
using Microsoft.AspNetCore.Http;

namespace gView.Server.Tests;

public class UrlTokenTests : IDisposable
{
    private const string Token = "token-abc~S3cr3tS3cr3tS3cr3tS3cr3t";

    private readonly string _root = Path.Combine(Path.GetTempPath(), "gview-urltoken-tests", Guid.NewGuid().ToString("N"));
    private string TokenFolder => Path.Combine(_root, "token");
    private string TokenFile => Path.Combine(TokenFolder, "token-abc.lgn");

    public UrlTokenTests()
    {
        Directory.CreateDirectory(TokenFolder);
        LoginStamp.ClearCache();
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, true); } catch { }
    }

    [Fact]
    public void Create_HashedUrlToken_LogsIn()
    {
        File.WriteAllText(TokenFile, SecureCrypto.HashSecret(Token));

        Assert.NotNull(AuthToken.Create(TokenFolder, "token-abc", Token, AuthToken.AuthTypes.Tokenuser));
        Assert.Null(AuthToken.Create(TokenFolder, "token-abc", Token + "x", AuthToken.AuthTypes.Tokenuser));
    }

    [Fact]
    public void Create_PlainUrlToken_LogsInAndIsHashed()
    {
        File.WriteAllText(TokenFile, Token);

        Assert.NotNull(AuthToken.Create(TokenFolder, "token-abc", Token, AuthToken.AuthTypes.Tokenuser));

        Assert.StartsWith(SecureCrypto.Sha256Prefix, File.ReadAllText(TokenFile));
        Assert.NotNull(AuthToken.Create(TokenFolder, "token-abc", Token, AuthToken.AuthTypes.Tokenuser));
    }

    [Fact]
    public void Create_HashValueAsPassword_IsRejected()
    {
        var hash = SecureCrypto.HashSecret(Token);
        File.WriteAllText(TokenFile, hash);

        Assert.Null(AuthToken.Create(TokenFolder, "token-abc", hash, AuthToken.AuthTypes.Tokenuser));
    }

    private async Task<HttpContext> Request(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        await UrlTokenAuthenticationMiddleware.AuthenticateAsync(context, _ => Task.CompletedTask, _root);
        return context;
    }

    [Fact]
    public async Task Middleware_ValidUrlToken_SetsUser()
    {
        File.WriteAllText(TokenFile, SecureCrypto.HashSecret(Token));

        var context = await Request($"/geoservices({Token})/rest/services");

        Assert.Equal("token-abc", context.User.ToAuthToken().Username);
        Assert.Equal(AuthToken.AuthTypes.Tokenuser, context.User.ToAuthToken().AuthType);
    }

    [Theory]
    [InlineData("/geoservices(token-abc~wrong)/rest/services")]
    [InlineData("/geoservices(token-unknown~x)/rest/services")]
    [InlineData("/geoservices(no-token)/rest/services")]
    public async Task Middleware_InvalidUrlToken_Throws(string path)
    {
        File.WriteAllText(TokenFile, SecureCrypto.HashSecret(Token));

        await Assert.ThrowsAsync<InvalidTokenException>(() => Request(path));
    }

    [Fact]
    public async Task Middleware_NoUrlToken_StaysAnonymous()
    {
        var context = await Request("/geoservices/rest/services");

        Assert.True(context.User.ToAuthToken().IsAnonymous);
    }

    [Theory]
    [InlineData("GET /geoservices/rest/services/x/MapServer/export?f=json&token=abc123&size=1",
                "GET /geoservices/rest/services/x/MapServer/export?f=json&token=***&size=1")]
    [InlineData("generateToken?username=u&password=geheim", "generateToken?username=u&password=***")]
    [InlineData("?access-token=xyz", "?access-token=***")]
    [InlineData("/geoservices(token-abc~S3cr3t)/rest/services", "/geoservices(token-abc~***)/rest/services")]
    [InlineData("{\"token\":\"abc\",\"x\":1}", "{\"token\":\"***\",\"x\":1}")]
    [InlineData("Authorization: Basic dXNlcjpwdw==", "Authorization: Basic ***")]
    [InlineData("Authorization: Bearer eyJhbGciOi.x.y", "Authorization: Bearer ***")]
    [InlineData("layers=show:0,1&tokenize=1", "layers=show:0,1&tokenize=1")]
    public void MaskSecrets_MasksTokensAndPasswords(string message, string expected)
    {
        Assert.Equal(expected, message.MaskSecrets());
    }
}
