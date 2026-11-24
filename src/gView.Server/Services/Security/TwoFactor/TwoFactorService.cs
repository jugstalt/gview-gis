using gView.Framework.Core.Exceptions;
using gView.Framework.Security;
using gView.Server.AppCode;
using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace gView.Server.Services.Security.TwoFactor;

/// <summary>
/// Two-factor login (TOTP) of the admin form login (security:manage:two-factor):
/// <list type="bullet">
/// <item><c>off</c>: never asked,</item>
/// <item><c>optional</c> (default): asked from admins who set it up,</item>
/// <item><c>required</c>: every admin has to set it up right after the password.</item>
/// </list>
/// Between password and code the browser holds a short-lived "pending" token
/// (bound to user and security stamp, no rights). Token users (Basic auth, /token,
/// url tokens, publishing) and OIDC admin logins never use it.
/// </summary>
public class TwoFactorService
{
    public static readonly TimeSpan PendingLifetime = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan SetupLifetime = TimeSpan.FromMinutes(15);
    public const int MaxPendingFailures = 5;

    private const string PendingPrefix = "2fa-pending";
    private const string SetupPrefix = "2fa-setup";

    private readonly TwoFactorStore _store;
    private readonly Func<X509CertificateWrapper> _certificate;
    private readonly ILogger _securityLog;
    private readonly ConcurrentDictionary<string, (int failures, DateTime expires)> _pendingFailures = new();
    private readonly ConcurrentDictionary<string, object> _userLocks = new(StringComparer.OrdinalIgnoreCase);

    public TwoFactorService(SecurityOptions securityOptions,
                            MapServer.MapServiceManager mapServiceManager,
                            EncryptionCertificateService encryptionCertificateService,
                            ILoggerFactory loggerFactory)
        : this(securityOptions.ManageTwoFactor,
               securityOptions.ManageTwoFactorIssuer,
               System.IO.Path.Combine(mapServiceManager.Options.LoginManagerRootPath ?? String.Empty, "manage"),
               () => encryptionCertificateService.GetCertificate(),
               loggerFactory?.CreateLogger(SecurityLog.Category))
    {
    }

    internal TwoFactorService(TwoFactorMode mode, string issuer, string manageFolder,
                              Func<X509CertificateWrapper> certificate, ILogger securityLog)
    {
        Mode = mode;
        Issuer = String.IsNullOrWhiteSpace(issuer) ? SecurityOptions.DefaultTwoFactorIssuer : issuer.Trim();
        _certificate = certificate;
        _store = new TwoFactorStore(manageFolder, certificate);
        _securityLog = securityLog;
    }

    public TwoFactorMode Mode { get; }
    public string Issuer { get; }

    internal TwoFactorStore Store => _store;

    /// <summary>For tests: the clock used for all decisions.</summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    #region State

    public bool IsEnabled(string username) => Mode != TwoFactorMode.Off && _store.IsEnabled(username);

    /// <summary>
    /// What follows a correct password of an admin form login.
    /// </summary>
    public TwoFactorStep StepAfterPassword(string username)
    {
        if (Mode == TwoFactorMode.Off)
        {
            return TwoFactorStep.None;
        }
        if (_store.IsEnabled(username))
        {
            return TwoFactorStep.Code;
        }
        return Mode == TwoFactorMode.Required ? TwoFactorStep.Setup : TwoFactorStep.None;
    }

    /// <summary>
    /// May an existing admin session (auth cookie) still be used? Not in required mode,
    /// if the admin has no second factor (e.g. sessions from before the switch).
    /// </summary>
    public bool IsSessionAllowed(string username)
        => Mode != TwoFactorMode.Required || _store.IsEnabled(username);

    public int RemainingRecoveryCodes(string username)
        => _store.Load(username)?.RecoveryCodeHashes.Count ?? 0;

    #endregion

    #region Verify

    /// <summary>
    /// Checks a TOTP code or a recovery code of a user with a second factor.
    /// A used recovery code is removed, the step of a used TOTP code is remembered (no replay).
    /// </summary>
    public TwoFactorVerifyResult Verify(string username, string code, string ip, bool allowRecoveryCode = true)
    {
        lock (UserLock(username))
        {
            var login = _store.Load(username);
            if (login is null)
            {
                return new TwoFactorVerifyResult(TwoFactorVerifyStatus.Invalid, 0);
            }

            if (allowRecoveryCode && RecoveryCodes.LooksLikeRecoveryCode(code))
            {
                int index = RecoveryCodes.Find(login.RecoveryCodeHashes, code);
                if (index < 0)
                {
                    _securityLog?.LogWarning("Wrong two-factor recovery code for user {username} from {ip}", username, ip);
                    return new TwoFactorVerifyResult(TwoFactorVerifyStatus.Invalid, login.RecoveryCodeHashes.Count);
                }

                login.RecoveryCodeHashes.RemoveAt(index);
                _store.Save(login);

                int remaining = login.RecoveryCodeHashes.Count;
                _securityLog?.LogWarning("Two-factor recovery code used by user {username} from {ip}, {remaining} left", username, ip, remaining);
                if (remaining <= RecoveryCodes.LowWarning)
                {
                    _securityLog?.LogWarning("Only {remaining} two-factor recovery codes left for user {username}", remaining, username);
                }

                return new TwoFactorVerifyResult(TwoFactorVerifyStatus.ValidRecoveryCode, remaining);
            }

            var result = Totp.Verify(login.Secret, code, UtcNow(), login.LastUsedStep, out long step);
            switch (result)
            {
                case TotpResult.Valid:
                    login.LastUsedStep = step;
                    _store.Save(login);
                    return new TwoFactorVerifyResult(TwoFactorVerifyStatus.Valid, login.RecoveryCodeHashes.Count);
                case TotpResult.Replay:
                    _securityLog?.LogWarning("Two-factor code of user {username} from {ip} refused: already used (replay)", username, ip);
                    return new TwoFactorVerifyResult(TwoFactorVerifyStatus.Replay, login.RecoveryCodeHashes.Count);
                default:
                    _securityLog?.LogWarning("Wrong two-factor code for user {username} from {ip}", username, ip);
                    return new TwoFactorVerifyResult(TwoFactorVerifyStatus.Invalid, login.RecoveryCodeHashes.Count);
            }
        }
    }

    #endregion

    #region Setup

    /// <summary>
    /// A new secret for the authenticator app. Nothing is stored: the secret travels
    /// in the encrypted setup token until the admin confirms it with a code.
    /// </summary>
    /// <param name="existingSetupToken">continue a setup (e.g. after a wrong code): same secret,
    /// the admin has already scanned the QR code</param>
    public TwoFactorSetup BeginSetup(string username, string host, string existingSetupToken = null)
    {
        var existing = ReadSetupToken(username, existingSetupToken);
        if (existing is not null)
        {
            return CreateSetup(username, host, existing.Value.secret, existingSetupToken);
        }

        var secret = Totp.NewSecret();
        var expires = UtcNow().Add(SetupLifetime).Ticks;

        return CreateSetup(username, host, secret, Protect($"{SetupPrefix}|{username}|{Totp.ToBase32(secret)}|{expires}"));
    }

    private TwoFactorSetup CreateSetup(string username, string host, byte[] secret, string token)
    {
        var account = String.IsNullOrWhiteSpace(host) ? username : $"{username}@{host}";
        var uri = Totp.OtpAuthUri(secret, Issuer, account);

        string svg;
        using (var generator = new QRCodeGenerator())
        using (var data = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.M))
        {
            svg = new SvgQRCode(data).GetGraphic(new System.Drawing.Size(220, 220), "#000000", "#ffffff",
                                                drawQuietZones: true,
                                                sizingMode: SvgQRCode.SizingMode.ViewBoxAttribute);
        }

        return new TwoFactorSetup(Totp.ToBase32(secret), uri, svg, token, account);
    }

    private (byte[] secret, long expires)? ReadSetupToken(string username, string setupToken)
    {
        var parts = Unprotect(setupToken, SetupPrefix, 4);
        if (parts is null
            || !String.Equals(username, parts[1], StringComparison.OrdinalIgnoreCase)
            || !long.TryParse(parts[3], out long expires) || expires < UtcNow().Ticks)
        {
            return null;
        }

        return (Totp.FromBase32(parts[2]), expires);
    }

    /// <summary>
    /// Activates (or replaces) the second factor, if the code matches the secret of
    /// the setup token. Ends all other sessions of the user (new security stamp).
    /// Returns the new recovery codes (shown once).
    /// </summary>
    public string[] ConfirmSetup(string username, string setupToken, string code, string ip)
    {
        var setup = ReadSetupToken(username, setupToken)
            ?? throw new MapServerException("The setup has expired. Please start again.");

        var secret = setup.secret;
        if (Totp.Verify(secret, code, UtcNow(), -1, out long step) != TotpResult.Valid)
        {
            _securityLog?.LogWarning("Wrong two-factor code while setting up user {username} from {ip}", username, ip);
            throw new MapServerException("Wrong code. Check the time of your phone and try again.");
        }

        var recoveryCodes = RecoveryCodes.Generate();

        lock (UserLock(username))
        {
            bool replaced = _store.IsEnabled(username);

            _store.Save(new TwoFactorLogin()
            {
                Username = username,
                Secret = secret,
                Activated = UtcNow(),
                LastUsedStep = step,
                RecoveryCodeHashes = recoveryCodes.Select(RecoveryCodes.Hash).ToList()
            });
            LoginStamp.Renew(_store.Folder, username);

            _securityLog?.LogWarning(replaced
                ? "Two-factor login of user {username} set up again (new device) from {ip}; all other sessions ended"
                : "Two-factor login enabled for user {username} from {ip}; all other sessions ended",
                username, ip);
        }

        return recoveryCodes;
    }

    /// <summary>
    /// New recovery codes; the old ones are no longer valid. Needs a current TOTP code.
    /// </summary>
    public string[] RegenerateRecoveryCodes(string username, string code, string ip)
    {
        RequireCode(username, code, ip, allowRecoveryCode: false);

        lock (UserLock(username))
        {
            var login = _store.Load(username) ?? throw new MapServerException("Two-factor login is not set up");
            var recoveryCodes = RecoveryCodes.Generate();

            login.RecoveryCodeHashes = recoveryCodes.Select(RecoveryCodes.Hash).ToList();
            _store.Save(login);

            _securityLog?.LogWarning("New two-factor recovery codes created for user {username} from {ip}", username, ip);

            return recoveryCodes;
        }
    }

    /// <summary>
    /// Throws, if the code (or recovery code) is not valid.
    /// </summary>
    public TwoFactorVerifyResult RequireCode(string username, string code, string ip, bool allowRecoveryCode = true)
    {
        var result = Verify(username, code, ip, allowRecoveryCode);

        return result.Status switch
        {
            TwoFactorVerifyStatus.Valid or TwoFactorVerifyStatus.ValidRecoveryCode => result,
            TwoFactorVerifyStatus.Replay => throw new MapServerException("This code was already used. Wait for the next code."),
            _ => throw new MapServerException("Wrong code")
        };
    }

    /// <summary>
    /// Switches the second factor off (only in optional mode; the caller checked password and code).
    /// </summary>
    public void Disable(string username, string ip)
    {
        if (Mode == TwoFactorMode.Required)
        {
            throw new MapServerException("Two-factor login is required on this server");
        }

        lock (UserLock(username))
        {
            if (_store.Delete(username))
            {
                LoginStamp.Renew(_store.Folder, username);
                _securityLog?.LogWarning("Two-factor login disabled for user {username} from {ip}; all other sessions ended", username, ip);
            }
        }
    }

    /// <summary>
    /// Emergency reset at startup (GVIEW_ADMIN_RESET_2FA = "user1,user2" or "*").
    /// </summary>
    public string[] ResetFromEnvironment(string value)
    {
        var names = (value ?? String.Empty)
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        if (names.Length == 0)
        {
            return [];
        }

        _securityLog?.LogWarning("{key} is set: two-factor login is reset for {users} on every start. Remove {key} after use!",
            TwoFactorKeys.ResetEnvironmentKey, String.Join(", ", names), TwoFactorKeys.ResetEnvironmentKey);

        var users = names.Contains("*")
            ? _store.Usernames().ToArray()
            : names.Select(n => n.ToLowerInvariant()).ToArray();

        var reset = users.Where(user =>
        {
            lock (UserLock(user))
            {
                if (!_store.Delete(user))
                {
                    return false;
                }
                LoginStamp.Renew(_store.Folder, user);
                _securityLog?.LogWarning("Two-factor login of user {username} reset by {key}; all sessions ended", user, TwoFactorKeys.ResetEnvironmentKey);
                return true;
            }
        }).ToArray();

        return reset;
    }

    #endregion

    #region Pending login (between password and code)

    public string CreatePendingToken(string username, string stamp, TwoFactorStep step)
    {
        var id = Guid.NewGuid().ToString("N");
        var expires = UtcNow().Add(PendingLifetime).Ticks;

        return Protect($"{PendingPrefix}|{id}|{username}|{expires}|{stamp}|{(int)step}");
    }

    /// <summary>
    /// The pending login of the token; null if invalid or expired.
    /// The caller checks, if the stamp is still current.
    /// </summary>
    public TwoFactorPending ReadPendingToken(string token)
    {
        var parts = Unprotect(token, PendingPrefix, 6);
        if (parts is null
            || !long.TryParse(parts[3], out long expires)
            || !int.TryParse(parts[5], out int step))
        {
            return null;
        }

        if (expires < UtcNow().Ticks)
        {
            _securityLog?.LogInformation("Pending two-factor login of user {username} expired", parts[2]);
            return null;
        }

        if (_pendingFailures.TryGetValue(parts[1], out var failures) && failures.failures >= MaxPendingFailures)
        {
            return null;
        }

        return new TwoFactorPending(parts[1], parts[2], parts[4], (TwoFactorStep)step);
    }

    /// <summary>
    /// Counts a wrong code of a pending login.
    /// </summary>
    /// <returns>true, if the pending login is dropped now (back to the password)</returns>
    public bool PendingFailed(TwoFactorPending pending)
    {
        var now = UtcNow();
        foreach (var stale in _pendingFailures.Where(p => p.Value.expires < now).Select(p => p.Key).ToArray())
        {
            _pendingFailures.TryRemove(stale, out _);
        }

        var entry = _pendingFailures.AddOrUpdate(pending.Id,
            _ => (1, now.Add(PendingLifetime)),
            (_, e) => (e.failures + 1, e.expires));

        if (entry.failures >= MaxPendingFailures)
        {
            _securityLog?.LogWarning("Pending two-factor login of user {username} dropped after {failures} wrong codes", pending.Username, entry.failures);
            return true;
        }
        return false;
    }

    public void PendingSucceeded(TwoFactorPending pending)
        => _pendingFailures.TryRemove(pending.Id, out _);

    #endregion

    #region Helper

    private object UserLock(string username) => _userLocks.GetOrAdd(username ?? String.Empty, _ => new object());

    private string Protect(string text) => SecureCrypto.EncryptToken(_certificate(), text, SecureCrypto.ResultType.Base62);

    private string[] Unprotect(string token, string prefix, int count)
    {
        if (String.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var parts = SecureCrypto.DecryptToken(_certificate(), token).Split('|');
            return parts.Length == count && parts[0] == prefix ? parts : null;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}

public enum TwoFactorMode
{
    Off,
    Optional,
    Required
}

public enum TwoFactorStep
{
    None = 0,
    Code = 1,
    Setup = 2
}

public enum TwoFactorVerifyStatus
{
    Invalid,
    Valid,
    ValidRecoveryCode,
    Replay
}

public record TwoFactorVerifyResult(TwoFactorVerifyStatus Status, int RemainingRecoveryCodes)
{
    public bool IsValid => Status is TwoFactorVerifyStatus.Valid or TwoFactorVerifyStatus.ValidRecoveryCode;
}

public record TwoFactorSetup(string Secret, string OtpAuthUri, string QrCodeSvg, string SetupToken, string Account);

public record TwoFactorPending(string Id, string Username, string Stamp, TwoFactorStep Step);

public static class TwoFactorKeys
{
    public const string ResetEnvironmentKey = "GVIEW_ADMIN_RESET_2FA";
    public const string PendingCookieName = "gview-2fa-pending";
}
