using gView.Framework.Security;
using gView.Server.AppCode;

namespace gView.Server.Tests;

public class AuthTokenTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "gview-authtoken-tests", Guid.NewGuid().ToString("N"));

    public AuthTokenTests()
    {
        Directory.CreateDirectory(_folder);
        PasswordVerificationCache.Clear();
    }

    public void Dispose()
    {
        try { Directory.Delete(_folder, true); } catch { }
    }

    [Fact]
    public void Create_LegacyHash_LogsInAndRehashesToPbkdf2()
    {
        var file = Path.Combine(_folder, "admin.lgn");
        File.WriteAllText(file, SecureCrypto.Hash64("secret", "admin"));

        var token = AuthToken.Create(_folder, "admin", "secret", AuthToken.AuthTypes.Manage);

        Assert.NotNull(token);
        Assert.StartsWith(SecureCrypto.Pbkdf2Prefix, File.ReadAllText(file));
        PasswordVerificationCache.Clear();
        Assert.NotNull(AuthToken.Create(_folder, "admin", "secret", AuthToken.AuthTypes.Manage));
    }

    [Fact]
    public void Create_WrongPassword_ReturnsNullAndKeepsFile()
    {
        var file = Path.Combine(_folder, "admin.lgn");
        var legacy = SecureCrypto.Hash64("secret", "admin");
        File.WriteAllText(file, legacy);

        Assert.Null(AuthToken.Create(_folder, "admin", "wrong", AuthToken.AuthTypes.Manage));
        Assert.Equal(legacy, File.ReadAllText(file));
    }

    [Fact]
    public void Create_AfterPasswordChange_OldPasswordIsNotCached()
    {
        var file = Path.Combine(_folder, "user1.lgn");
        File.WriteAllText(file, SecureCrypto.HashPassword("old-password"));
        Assert.NotNull(AuthToken.Create(_folder, "user1", "old-password", AuthToken.AuthTypes.Tokenuser));

        File.WriteAllText(file, SecureCrypto.HashPassword("new-password"));

        Assert.Null(AuthToken.Create(_folder, "user1", "old-password", AuthToken.AuthTypes.Tokenuser));
        Assert.NotNull(AuthToken.Create(_folder, "user1", "new-password", AuthToken.AuthTypes.Tokenuser));
    }

    [Fact]
    public void Create_PathTraversalUsername_ReturnsNull()
    {
        var manage = Path.Combine(_folder, "manage");
        Directory.CreateDirectory(manage);
        File.WriteAllText(Path.Combine(manage, "admin.lgn"), SecureCrypto.HashPassword("secret"));
        var token = Path.Combine(_folder, "token");
        Directory.CreateDirectory(token);

        Assert.Null(AuthToken.Create(token, "../manage/admin", "secret", AuthToken.AuthTypes.Tokenuser));
        Assert.Null(AuthToken.Create(token, Path.Combine(manage, "admin"), "secret", AuthToken.AuthTypes.Tokenuser));
    }

    [Fact]
    public void Create_UrlToken_ComparesValue()
    {
        File.WriteAllText(Path.Combine(_folder, "token-abc.lgn"), "token-abc~xyz");

        Assert.Null(AuthToken.Create(_folder, "token-abc", "token-abc~xy", AuthToken.AuthTypes.Tokenuser));
        Assert.NotNull(AuthToken.Create(_folder, "token-abc", "token-abc~xyz", AuthToken.AuthTypes.Tokenuser));
    }
}
