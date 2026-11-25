using gView.Framework.Core.Exceptions;
using gView.Framework.Security;
using gView.Server.AppCode;
using gView.Server.Services.Security.TwoFactor;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace gView.Server.Tests;

public class TwoFactorServiceTests : IDisposable
{
    private static readonly Lazy<X509CertificateWrapper> Certificate = new(() =>
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("cn=gview-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

        return new X509CertificateWrapper(X509CertificateLoader.LoadPkcs12(cert.Export(X509ContentType.Pfx), null,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet));
    });

    private readonly string _folder = Path.Combine(Path.GetTempPath(), "gview-2fa-tests", Guid.NewGuid().ToString("N"));
    private DateTime _now = new(2026, 11, 24, 12, 0, 10, DateTimeKind.Utc);

    public TwoFactorServiceTests()
    {
        Directory.CreateDirectory(_folder);
        LoginStamp.ClearCache();
        File.WriteAllText(Path.Combine(_folder, "admin.lgn"), SecureCrypto.HashPassword("secret"));
        File.WriteAllText(Path.Combine(_folder, "other.lgn"), SecureCrypto.HashPassword("secret"));
    }

    public void Dispose()
    {
        try { Directory.Delete(_folder, true); } catch { }
    }

    private TwoFactorService Service(TwoFactorMode mode = TwoFactorMode.Optional)
        => new(mode, null!, _folder, () => Certificate.Value, null!) { UtcNow = () => _now };

    private string CodeFor(string base32Secret, int stepOffset = 0)
        => Totp.Code(Totp.FromBase32(base32Secret), Totp.TimeStep(_now) + stepOffset);

    private (TwoFactorService service, string secret, string[] recoveryCodes) Enabled(string username = "admin", TwoFactorMode mode = TwoFactorMode.Optional)
    {
        var service = Service(mode);
        var setup = service.BeginSetup(username, "maps.example.com");
        var codes = service.ConfirmSetup(username, setup.SetupToken, CodeFor(setup.Secret), "127.0.0.1");
        _now = _now.AddSeconds(Totp.StepSeconds);  // the setup code's step is used

        return (service, setup.Secret, codes);
    }

    #region Setup

    [Fact]
    public void BeginSetup_StoresNothing()
    {
        var service = Service();

        var setup = service.BeginSetup("admin", "maps.example.com");

        Assert.False(service.IsEnabled("admin"));
        Assert.False(File.Exists(Path.Combine(_folder, "admin.2fa")));
        Assert.StartsWith("<svg", setup.QrCodeSvg.TrimStart());
        Assert.Equal("admin@maps.example.com", setup.Account);
        Assert.Contains(setup.Secret, setup.OtpAuthUri);
    }

    [Fact]
    public void ConfirmSetup_ValidCode_EnablesAndReturnsRecoveryCodes()
    {
        var service = Service();
        var stampBefore = LoginStamp.Current(_folder, "admin");
        var setup = service.BeginSetup("admin", "host");

        var codes = service.ConfirmSetup("admin", setup.SetupToken, CodeFor(setup.Secret), "ip");

        Assert.True(service.IsEnabled("admin"));
        Assert.Equal(RecoveryCodes.Count, codes.Length);
        Assert.Equal(RecoveryCodes.Count, service.RemainingRecoveryCodes("admin"));
        Assert.NotEqual(stampBefore, LoginStamp.Current(_folder, "admin"));  // other sessions end
    }

    [Fact]
    public void ConfirmSetup_WrongCode_Throws()
    {
        var service = Service();
        var setup = service.BeginSetup("admin", "host");

        Assert.Throws<MapServerException>(() => service.ConfirmSetup("admin", setup.SetupToken, "000000", "ip"));
        Assert.False(service.IsEnabled("admin"));
    }

    [Fact]
    public void ConfirmSetup_ExpiredToken_Throws()
    {
        var service = Service();
        var setup = service.BeginSetup("admin", "host");
        _now = _now.Add(TwoFactorService.SetupLifetime).AddMinutes(1);

        Assert.Throws<MapServerException>(() => service.ConfirmSetup("admin", setup.SetupToken, CodeFor(setup.Secret), "ip"));
    }

    [Fact]
    public void ConfirmSetup_AfterSignOutEverywhere_Throws()
    {
        var service = Service();
        var setup = service.BeginSetup("admin", "host");
        LoginStamp.Renew(_folder, "admin");

        Assert.Throws<MapServerException>(() => service.ConfirmSetup("admin", setup.SetupToken, CodeFor(setup.Secret), "ip"));
    }

    [Fact]
    public void ConfirmSetup_TokenOfOtherUser_Throws()
    {
        var service = Service();
        var setup = service.BeginSetup("other", "host");

        Assert.Throws<MapServerException>(() => service.ConfirmSetup("admin", setup.SetupToken, CodeFor(setup.Secret), "ip"));
    }

    [Fact]
    public void BeginSetup_WithExistingToken_KeepsSecret()
    {
        var service = Service();
        var setup = service.BeginSetup("admin", "host");

        var resumed = service.BeginSetup("admin", "host", setup.SetupToken);

        Assert.Equal(setup.Secret, resumed.Secret);
    }

    [Fact]
    public void Store_SecretIsEncryptedOnDisk_AndRoundtrips()
    {
        var (service, secret, _) = Enabled();

        var json = File.ReadAllText(Path.Combine(_folder, "admin.2fa"));
        var login = service.Store.Load("admin");

        Assert.DoesNotContain(secret, json);
        Assert.Equal(Totp.FromBase32(secret), login.Secret);
    }

    #endregion

    #region Verify

    [Fact]
    public void Verify_ValidCode_ThenReplayRefused()
    {
        var (service, secret, _) = Enabled();
        var code = CodeFor(secret);

        Assert.Equal(TwoFactorVerifyStatus.Valid, service.Verify("admin", code, "ip").Status);
        Assert.Equal(TwoFactorVerifyStatus.Replay, service.Verify("admin", code, "ip").Status);
    }

    [Fact]
    public void Verify_SetupCodeCannotBeReplayed()
    {
        var service = Service();
        var setup = service.BeginSetup("admin", "host");
        var code = CodeFor(setup.Secret);
        service.ConfirmSetup("admin", setup.SetupToken, code, "ip");

        Assert.Equal(TwoFactorVerifyStatus.Replay, service.Verify("admin", code, "ip").Status);
    }

    [Fact]
    public void Verify_WrongCode_IsInvalid()
    {
        var (service, secret, _) = Enabled();

        Assert.Equal(TwoFactorVerifyStatus.Invalid, service.Verify("admin", CodeFor(secret, 3), "ip").Status);
    }

    [Fact]
    public void Verify_RecoveryCode_WorksOnce()
    {
        var (service, _, codes) = Enabled();

        var first = service.Verify("admin", codes[0], "ip");
        var second = service.Verify("admin", codes[0], "ip");

        Assert.Equal(TwoFactorVerifyStatus.ValidRecoveryCode, first.Status);
        Assert.Equal(RecoveryCodes.Count - 1, first.RemainingRecoveryCodes);
        Assert.Equal(TwoFactorVerifyStatus.Invalid, second.Status);
    }

    [Fact]
    public void Verify_RecoveryCodeNotAllowed_IsInvalid()
    {
        var (service, _, codes) = Enabled();

        Assert.Equal(TwoFactorVerifyStatus.Invalid, service.Verify("admin", codes[0], "ip", allowRecoveryCode: false).Status);
        Assert.Equal(RecoveryCodes.Count, service.RemainingRecoveryCodes("admin"));
    }

    [Fact]
    public void Verify_UserWithoutSecondFactor_IsInvalid()
    {
        Assert.Equal(TwoFactorVerifyStatus.Invalid, Service().Verify("admin", "123456", "ip").Status);
    }

    #endregion

    #region Manage

    [Fact]
    public void RegenerateRecoveryCodes_OldCodesInvalid()
    {
        var (service, secret, oldCodes) = Enabled();

        var newCodes = service.RegenerateRecoveryCodes("admin", CodeFor(secret), "ip");

        Assert.Equal(RecoveryCodes.Count, newCodes.Length);
        Assert.Equal(TwoFactorVerifyStatus.Invalid, service.Verify("admin", oldCodes[0], "ip").Status);
        Assert.Equal(TwoFactorVerifyStatus.ValidRecoveryCode, service.Verify("admin", newCodes[0], "ip").Status);
    }

    [Fact]
    public void RegenerateRecoveryCodes_WithRecoveryCode_Throws()
    {
        var (service, _, codes) = Enabled();

        Assert.Throws<MapServerException>(() => service.RegenerateRecoveryCodes("admin", codes[0], "ip"));
    }

    [Fact]
    public void ConfirmSetup_Replace_NewSecretAndStamp()
    {
        var (service, oldSecret, _) = Enabled();
        var stampBefore = LoginStamp.Current(_folder, "admin");

        var setup = service.BeginSetup("admin", "host");
        service.ConfirmSetup("admin", setup.SetupToken, CodeFor(setup.Secret), "ip");
        _now = _now.AddSeconds(Totp.StepSeconds);

        Assert.NotEqual(oldSecret, setup.Secret);
        Assert.NotEqual(stampBefore, LoginStamp.Current(_folder, "admin"));
        Assert.Equal(TwoFactorVerifyStatus.Valid, service.Verify("admin", CodeFor(setup.Secret), "ip").Status);
    }

    [Fact]
    public void Disable_Optional_RemovesFileAndRenewsStamp()
    {
        var (service, _, _) = Enabled();
        var stampBefore = LoginStamp.Current(_folder, "admin");

        service.Disable("admin", "ip");

        Assert.False(service.IsEnabled("admin"));
        Assert.NotEqual(stampBefore, LoginStamp.Current(_folder, "admin"));
    }

    [Fact]
    public void Disable_Required_Throws()
    {
        var (service, _, _) = Enabled(mode: TwoFactorMode.Required);

        Assert.Throws<MapServerException>(() => service.Disable("admin", "ip"));
        Assert.True(service.IsEnabled("admin"));
    }

    #endregion

    #region Mode

    [Theory]
    [InlineData(TwoFactorMode.Off, false, TwoFactorStep.None)]
    [InlineData(TwoFactorMode.Off, true, TwoFactorStep.None)]
    [InlineData(TwoFactorMode.Optional, false, TwoFactorStep.None)]
    [InlineData(TwoFactorMode.Optional, true, TwoFactorStep.Code)]
    [InlineData(TwoFactorMode.Required, false, TwoFactorStep.Setup)]
    [InlineData(TwoFactorMode.Required, true, TwoFactorStep.Code)]
    public void StepAfterPassword_DependsOnModeAndSetup(TwoFactorMode mode, bool enabled, TwoFactorStep expected)
    {
        if (enabled)
        {
            Enabled();
        }

        Assert.Equal(expected, Service(mode).StepAfterPassword("admin"));
    }

    [Theory]
    [InlineData(TwoFactorMode.Off, false, true)]
    [InlineData(TwoFactorMode.Optional, false, true)]
    [InlineData(TwoFactorMode.Required, false, false)]
    [InlineData(TwoFactorMode.Required, true, true)]
    public void IsSessionAllowed_RequiredNeedsSecondFactor(TwoFactorMode mode, bool enabled, bool expected)
    {
        if (enabled)
        {
            Enabled();
        }

        Assert.Equal(expected, Service(mode).IsSessionAllowed("admin"));
    }

    #endregion

    #region Pending

    [Fact]
    public void PendingToken_Roundtrip()
    {
        var service = Service();

        var pending = service.ReadPendingToken(service.CreatePendingToken("admin", "stamp1", TwoFactorStep.Code));

        Assert.NotNull(pending);
        Assert.Equal("admin", pending.Username);
        Assert.Equal("stamp1", pending.Stamp);
        Assert.Equal(TwoFactorStep.Code, pending.Step);
    }

    [Fact]
    public void PendingToken_Expired_IsNull()
    {
        var service = Service();
        var token = service.CreatePendingToken("admin", "stamp1", TwoFactorStep.Code);

        _now = _now.Add(TwoFactorService.PendingLifetime).AddSeconds(1);

        Assert.Null(service.ReadPendingToken(token));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("garbage")]
    public void PendingToken_Invalid_IsNull(string? token)
    {
        Assert.Null(Service().ReadPendingToken(token!));
    }

    [Fact]
    public void PendingToken_OtherTokenKind_IsNull()
    {
        var service = Service();
        var setupToken = service.BeginSetup("admin", "host").SetupToken;

        Assert.Null(service.ReadPendingToken(setupToken));
    }

    [Fact]
    public void PendingToken_DroppedAfterMaxFailures()
    {
        var service = Service();
        var token = service.CreatePendingToken("admin", "stamp1", TwoFactorStep.Code);
        var pending = service.ReadPendingToken(token)!;

        for (int i = 1; i < TwoFactorService.MaxPendingFailures; i++)
        {
            Assert.False(service.PendingFailed(pending));
        }

        Assert.True(service.PendingFailed(pending));
        Assert.Null(service.ReadPendingToken(token));
    }

    #endregion

    #region Environment reset

    [Fact]
    public void ResetFromEnvironment_List_ResetsNamedUsers()
    {
        Enabled("admin");
        Enabled("other");
        var stampBefore = LoginStamp.Current(_folder, "admin");
        var service = Service();

        var reset = service.ResetFromEnvironment(" Admin , unknown");

        Assert.Equal(["admin"], reset);
        Assert.False(service.IsEnabled("admin"));
        Assert.True(service.IsEnabled("other"));
        Assert.NotEqual(stampBefore, LoginStamp.Current(_folder, "admin"));
    }

    [Fact]
    public void ResetFromEnvironment_Star_ResetsAll()
    {
        Enabled("admin");
        Enabled("other");
        var service = Service();

        var reset = service.ResetFromEnvironment("*");

        Assert.Equal(2, reset.Length);
        Assert.False(service.IsEnabled("admin"));
        Assert.False(service.IsEnabled("other"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" , ")]
    public void ResetFromEnvironment_Empty_ResetsNothing(string value)
    {
        Enabled();

        Assert.Empty(Service().ResetFromEnvironment(value));
        Assert.True(Service().IsEnabled("admin"));
    }

    [Fact]
    public void ResetFromEnvironment_PathTraversal_Ignored()
    {
        Enabled();

        Assert.Empty(Service().ResetFromEnvironment("../admin"));
        Assert.True(Service().IsEnabled("admin"));
    }

    #endregion
}
