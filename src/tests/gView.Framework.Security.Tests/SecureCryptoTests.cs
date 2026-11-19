namespace gView.Framework.Security.Tests;

public class SecureCryptoTests
{
    [Fact]
    public void HashPassword_ProducesPbkdf2Format_AndVerifies()
    {
        var hash = SecureCrypto.HashPassword("Correct-Horse-42");

        var ok = SecureCrypto.VerifyPassword("Correct-Horse-42", hash, "admin", out bool needsRehash);

        Assert.StartsWith(SecureCrypto.Pbkdf2Prefix, hash);
        Assert.True(ok);
        Assert.False(needsRehash);
    }

    [Fact]
    public void HashPassword_UsesRandomSalt()
    {
        Assert.NotEqual(SecureCrypto.HashPassword("same"), SecureCrypto.HashPassword("same"));
    }

    [Fact]
    public void VerifyPassword_Pbkdf2WrongPassword_ReturnsFalse()
    {
        var hash = SecureCrypto.HashPassword("Correct-Horse-42");

        Assert.False(SecureCrypto.VerifyPassword("wrong", hash, "admin", out _));
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("Admin ")]
    public void VerifyPassword_LegacyHash_MatchesAndNeedsRehash(string username)
    {
        var legacy = SecureCrypto.Hash64("secret", "admin");

        var ok = SecureCrypto.VerifyPassword("secret", legacy, username, out bool needsRehash);

        Assert.True(ok);
        Assert.True(needsRehash);
    }

    [Fact]
    public void VerifyPassword_LegacyHashOtherUser_ReturnsFalse()
    {
        var legacy = SecureCrypto.Hash64("secret", "admin");

        Assert.False(SecureCrypto.VerifyPassword("secret", legacy, "other", out _));
    }

    [Fact]
    public void VerifyPassword_LegacyOverload_StillWorks()
    {
        Assert.True(SecureCrypto.VerifyPassword("secret", SecureCrypto.Hash64("secret", "admin"), "admin"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("pbkdf2-sha256$")]
    [InlineData("pbkdf2-sha256$abc$AAAA$AAAA")]
    [InlineData("pbkdf2-sha256$1000$not-base64$AAAA")]
    public void VerifyPassword_MalformedHash_ReturnsFalse(string hash)
    {
        Assert.False(SecureCrypto.VerifyPassword("secret", hash, "admin", out _));
    }

    [Theory]
    [InlineData("abc", "abc", true)]
    [InlineData("abc", "abd", false)]
    [InlineData("abc", "abcd", false)]
    [InlineData(null, "abc", false)]
    public void FixedTimeEquals_ComparesStrings(string? a, string? b, bool expected)
    {
        Assert.Equal(expected, SecureCrypto.FixedTimeEquals(a!, b!));
    }
}
