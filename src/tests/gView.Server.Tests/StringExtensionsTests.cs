using gView.Server.Extensions;

namespace gView.Server.Tests;

public class StringExtensionsTests
{
    private static readonly string Folder = Path.Combine(Path.GetTempPath(), "gview-login-tests", "token");

    [Theory]
    [InlineData("admin")]
    [InlineData("user_1-x")]
    [InlineData("token-abc")]
    public void ToLoginFile_ValidUsername_ReturnsFileInFolder(string username)
    {
        var fi = username.ToLoginFile(Folder);

        Assert.NotNull(fi);
        Assert.Equal(Path.GetFullPath(Folder), fi!.DirectoryName);
        Assert.Equal($"{username}.lgn", fi.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("..")]
    [InlineData("../manage/admin")]
    [InlineData("..\\manage\\admin")]
    [InlineData("sub/admin")]
    [InlineData("C:\\temp\\x")]
    [InlineData("/etc/passwd")]
    [InlineData("c:x")]
    [InlineData("a\0b")]
    public void ToLoginFile_UnsafeUsername_ReturnsNull(string username)
    {
        Assert.Null(username.ToLoginFile(Folder));
    }

    [Fact]
    public void ToLoginFile_TooLongUsername_ReturnsNull()
    {
        Assert.Null(new string('a', 257).ToLoginFile(Folder));
    }
}
