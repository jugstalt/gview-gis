using gView.Server.Services.Security.TwoFactor;
using System.Text;

namespace gView.Server.Tests;

public class TotpTests
{
    private static readonly byte[] RfcSecret = Encoding.ASCII.GetBytes("12345678901234567890");

    [Theory]  // RFC 6238, Appendix B (SHA-1, 8 digits)
    [InlineData(59L, "94287082")]
    [InlineData(1111111109L, "07081804")]
    [InlineData(1111111111L, "14050471")]
    [InlineData(1234567890L, "89005924")]
    [InlineData(2000000000L, "69279037")]
    [InlineData(20000000000L, "65353130")]
    public void Code_Rfc6238TestVectors_Match(long unixSeconds, string expected)
    {
        var step = Totp.TimeStep(DateTime.UnixEpoch.AddSeconds(unixSeconds));

        Assert.Equal(expected, Totp.Code(RfcSecret, step, digits: 8));
    }

    [Fact]
    public void Code_SixDigits_IsLastSixOfRfcValue()
    {
        var step = Totp.TimeStep(DateTime.UnixEpoch.AddSeconds(59));

        Assert.Equal("287082", Totp.Code(RfcSecret, step));
    }

    [Fact]
    public void Base32_Roundtrip()
    {
        var secret = Totp.NewSecret();

        var base32 = Totp.ToBase32(secret);

        Assert.Equal(32, base32.Length);  // 160 bit
        Assert.Equal(secret, Totp.FromBase32(base32));
        Assert.Equal(secret, Totp.FromBase32(base32.ToLowerInvariant()));
    }

    [Fact]
    public void Base32_KnownValue()
    {
        Assert.Equal("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", Totp.ToBase32(RfcSecret));
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(-2, false)]
    [InlineData(2, false)]
    public void Verify_AcceptsOneStepDrift(int offset, bool valid)
    {
        var now = new DateTime(2026, 11, 24, 12, 0, 10, DateTimeKind.Utc);
        var code = Totp.Code(RfcSecret, Totp.TimeStep(now) + offset);

        Assert.Equal(valid ? TotpResult.Valid : TotpResult.Invalid, Totp.Verify(RfcSecret, code, now, lastUsedStep: 0, out _));
    }

    [Fact]
    public void Verify_SameOrOlderStepThanLastUsed_IsReplay()
    {
        var now = new DateTime(2026, 11, 24, 12, 0, 10, DateTimeKind.Utc);
        var step = Totp.TimeStep(now);
        var code = Totp.Code(RfcSecret, step);

        Assert.Equal(TotpResult.Valid, Totp.Verify(RfcSecret, code, now, step - 1, out var used));
        Assert.Equal(step, used);
        Assert.Equal(TotpResult.Replay, Totp.Verify(RfcSecret, code, now, step, out _));
        Assert.Equal(TotpResult.Replay, Totp.Verify(RfcSecret, Totp.Code(RfcSecret, step - 1), now, step, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12a456")]
    public void Verify_MalformedCode_IsInvalid(string? code)
    {
        Assert.Equal(TotpResult.Invalid, Totp.Verify(RfcSecret, code!, DateTime.UtcNow, 0, out _));
    }

    [Fact]
    public void Verify_CodeWithSpaces_IsAccepted()
    {
        var now = DateTime.UtcNow;
        var code = Totp.Code(RfcSecret, Totp.TimeStep(now));

        Assert.Equal(TotpResult.Valid, Totp.Verify(RfcSecret, $"{code[..3]} {code[3..]}", now, 0, out _));
    }

    [Fact]
    public void OtpAuthUri_ContainsParameters()
    {
        var uri = Totp.OtpAuthUri(RfcSecret, "gView Server", "admin@maps.example.com");

        Assert.StartsWith("otpauth://totp/gView%20Server:admin%40maps.example.com?", uri);
        Assert.Contains("secret=GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", uri);
        Assert.Contains("issuer=gView%20Server", uri);
        Assert.Contains("digits=6", uri);
        Assert.Contains("period=30", uri);
    }
}
