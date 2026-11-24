using gView.Framework.Security;
using gView.Server.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Server.Services.Security.TwoFactor;

/// <summary>
/// The second factor of an admin login: <c>&lt;user&gt;.2fa</c> next to its
/// <c>&lt;user&gt;.lgn</c> (JSON). The TOTP secret is encrypted with the server
/// certificate (crypto0), the recovery codes are hashed.
/// Deleting the file switches the second factor off for this user (emergency reset).
/// </summary>
internal class TwoFactorStore
{
    public const string FileExtension = ".2fa";

    private static readonly TimeSpan CacheTime = TimeSpan.FromSeconds(10);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _folder;
    private readonly Func<X509CertificateWrapper> _certificate;
    private readonly ConcurrentDictionary<string, (bool exists, DateTime expires)> _existsCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _writeLock = new();

    public TwoFactorStore(string folder, Func<X509CertificateWrapper> certificate)
    {
        _folder = folder;
        _certificate = certificate;
    }

    public string Folder => _folder;

    /// <summary>
    /// Has the user a second factor? Cached for a few seconds (checked on every admin request).
    /// </summary>
    public bool IsEnabled(string username)
    {
        var file = File(username);
        if (file is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        if (_existsCache.TryGetValue(file.FullName, out var cached) && cached.expires > now)
        {
            return cached.exists;
        }

        file.Refresh();
        _existsCache[file.FullName] = (file.Exists, now.Add(CacheTime));

        return file.Exists;
    }

    public TwoFactorLogin Load(string username)
    {
        var file = File(username);
        if (file is null || !file.Exists)
        {
            return null;
        }

        var data = JsonSerializer.Deserialize<FileModel>(System.IO.File.ReadAllText(file.FullName), JsonOptions);
        if (data is null || !"totp".Equals(data.Method, StringComparison.OrdinalIgnoreCase) || String.IsNullOrEmpty(data.Secret))
        {
            throw new InvalidDataException($"Invalid two-factor file {file.Name}");
        }

        return new TwoFactorLogin()
        {
            Username = username,
            Secret = Totp.FromBase32(SecureCrypto.DecryptToken(_certificate(), data.Secret)),
            Activated = data.Activated,
            LastUsedStep = data.LastStep,
            RecoveryCodeHashes = data.RecoveryCodes?.ToList() ?? []
        };
    }

    public void Save(TwoFactorLogin login)
    {
        var file = File(login.Username) ?? throw new ArgumentException("Invalid username");

        var data = new FileModel()
        {
            Version = 1,
            Method = "totp",
            Secret = SecureCrypto.EncryptToken(_certificate(), Totp.ToBase32(login.Secret)),
            Activated = login.Activated,
            LastStep = login.LastUsedStep,
            RecoveryCodes = login.RecoveryCodeHashes.ToArray()
        };

        lock (_writeLock)
        {
            var temp = file.FullName + ".tmp";
            System.IO.File.WriteAllText(temp, JsonSerializer.Serialize(data, JsonOptions));
            System.IO.File.Move(temp, file.FullName, overwrite: true);
        }

        _existsCache.TryRemove(file.FullName, out _);
    }

    /// <returns>true, if a file was deleted</returns>
    public bool Delete(string username)
    {
        var file = File(username);
        if (file is null)
        {
            return false;
        }

        _existsCache.TryRemove(file.FullName, out _);
        file.Refresh();
        if (!file.Exists)
        {
            return false;
        }

        file.Delete();
        return true;
    }

    /// <summary>
    /// All users with a second factor.
    /// </summary>
    public IEnumerable<string> Usernames()
    {
        var di = new DirectoryInfo(_folder);
        return di.Exists
            ? di.GetFiles($"*{FileExtension}").Select(f => f.Name[..^FileExtension.Length])
            : [];
    }

    private FileInfo File(string username)
    {
        var loginFile = username?.Trim().ToLowerInvariant().ToLoginFile(_folder);  // validates the name
        return loginFile is null
            ? null
            : new FileInfo(Path.ChangeExtension(loginFile.FullName, FileExtension));
    }

    private class FileModel
    {
        [JsonPropertyName("version")] public int Version { get; set; }
        [JsonPropertyName("method")] public string Method { get; set; }
        [JsonPropertyName("secret")] public string Secret { get; set; }
        [JsonPropertyName("activated")] public DateTime Activated { get; set; }
        [JsonPropertyName("last-step")] public long LastStep { get; set; }
        [JsonPropertyName("recovery-codes")] public string[] RecoveryCodes { get; set; }
    }
}

internal class TwoFactorLogin
{
    public string Username { get; init; }
    public byte[] Secret { get; init; }
    public DateTime Activated { get; init; }
    public long LastUsedStep { get; set; }
    public List<string> RecoveryCodeHashes { get; set; } = [];
}
