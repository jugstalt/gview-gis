using gView.Framework.Security;
using gView.Server.AppCode;

namespace gView.Server.Tests;

public class LoginStampTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "gview-stamp-tests", Guid.NewGuid().ToString("N"));

    public LoginStampTests()
    {
        Directory.CreateDirectory(_folder);
        LoginStamp.ClearCache();
        PasswordVerificationCache.Clear();
        File.WriteAllText(Path.Combine(_folder, "admin.lgn"), SecureCrypto.HashPassword("secret"));
    }

    public void Dispose()
    {
        try { Directory.Delete(_folder, true); } catch { }
    }

    private AuthToken Login(string password = "secret")
        => AuthToken.Create(_folder, "admin", password, AuthToken.AuthTypes.Manage)!;

    [Fact]
    public void Login_TokenCarriesCurrentStamp()
    {
        var token = Login();

        Assert.NotNull(token.Stamp);
        Assert.Equal(LoginStamp.Current(_folder, "admin"), token.Stamp);
    }

    [Fact]
    public void Stamp_IsStableBetweenLogins()
    {
        Assert.Equal(Login().Stamp, Login().Stamp);
    }

    [Fact]
    public void Renew_ChangesStamp_OldTokensNoLongerCurrent()
    {
        var token = Login();

        LoginStamp.Renew(_folder, "admin");

        Assert.NotEqual(token.Stamp, LoginStamp.Current(_folder, "admin"));
        Assert.Equal(LoginStamp.Current(_folder, "admin"), Login().Stamp);
    }

    [Fact]
    public void PasswordChange_ChangesStamp()
    {
        var token = Login();

        var file = new FileInfo(Path.Combine(_folder, "admin.lgn"));
        File.WriteAllText(file.FullName, SecureCrypto.HashPassword("new-secret"));
        LoginStamp.Invalidate(file);

        Assert.NotEqual(token.Stamp, LoginStamp.Current(_folder, "admin"));
    }

    [Fact]
    public void LegacyRehashOnLogin_StampMatchesTheRehashedFile()
    {
        File.WriteAllText(Path.Combine(_folder, "legacy.lgn"), SecureCrypto.Hash64("secret", "legacy"));
        var before = LoginStamp.Current(_folder, "legacy");

        var token = AuthToken.Create(_folder, "legacy", "secret", AuthToken.AuthTypes.Manage)!;

        Assert.NotEqual(before, token.Stamp);
        Assert.Equal(LoginStamp.Current(_folder, "legacy"), token.Stamp);
    }

    [Fact]
    public void Delete_RemovesStampFile_UnknownUserHasNoStamp()
    {
        LoginStamp.Renew(_folder, "admin");
        var file = new FileInfo(Path.Combine(_folder, "admin.lgn"));
        file.Delete();

        LoginStamp.Delete(file);

        Assert.False(File.Exists(Path.Combine(_folder, "admin.stamp")));
        Assert.Null(LoginStamp.Current(_folder, "admin"));
        Assert.Null(LoginStamp.Current(_folder, "../admin"));
    }
}
