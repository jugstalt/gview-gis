using gView.Server.Services.Security.TwoFactor;

namespace gView.Server.Tests;

public class RecoveryCodesTests
{
    [Fact]
    public void Generate_TenDistinctCodesInFormat()
    {
        var codes = RecoveryCodes.Generate();

        Assert.Equal(RecoveryCodes.Count, codes.Length);
        Assert.Equal(codes.Length, codes.Distinct().Count());
        Assert.All(codes, c => Assert.Matches("^[a-z2-7]{5}-[a-z2-7]{5}$", c));
    }

    [Fact]
    public void Find_MatchesHashedCode_IgnoringCaseAndSeparator()
    {
        var codes = RecoveryCodes.Generate(3);
        var hashes = codes.Select(RecoveryCodes.Hash).ToList();

        Assert.Equal(1, RecoveryCodes.Find(hashes, codes[1]));
        Assert.Equal(2, RecoveryCodes.Find(hashes, codes[2].Replace("-", "").ToUpperInvariant()));
        Assert.Equal(-1, RecoveryCodes.Find(hashes, "aaaaa-aaaaa"));
    }

    [Fact]
    public void Hash_DoesNotContainCode()
    {
        var code = RecoveryCodes.Generate(1)[0];

        Assert.DoesNotContain(code.Replace("-", ""), RecoveryCodes.Hash(code));
    }

    [Theory]
    [InlineData("abcde-fghij", true)]
    [InlineData("ABCDEFGHIJ", true)]
    [InlineData("123456", false)]
    [InlineData("", false)]
    public void LooksLikeRecoveryCode(string input, bool expected)
    {
        Assert.Equal(expected, RecoveryCodes.LooksLikeRecoveryCode(input));
    }
}
