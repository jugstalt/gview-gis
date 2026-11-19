using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Extensions;
using gView.Framework.Core.MapServer;

namespace gView.Framework.Core.Tests.Extensions;

public class ExceptionExtensionsTests
{
    [Fact]
    public void NewErrorId_IsEightLowerCaseHexChars()
    {
        string id = ExceptionExtensions.NewErrorId();

        Assert.Matches("^[0-9a-f]{8}$", id);
    }

    [Fact]
    public void NewErrorId_ReturnsDifferentIds()
    {
        var ids = Enumerable.Range(0, 100).Select(_ => ExceptionExtensions.NewErrorId()).ToHashSet();

        Assert.True(ids.Count > 95);
    }

    [Theory]
    [InlineData("abc12345", "internal server error (id: abc12345)")]
    [InlineData(null, "internal server error")]
    [InlineData("", "internal server error")]
    public void InternalErrorMessage_ContainsIdIfGiven(string? errorId, string expected)
    {
        Assert.Equal(expected, ExceptionExtensions.InternalErrorMessage(errorId!));
    }

    [Fact]
    public void ToClientMessage_MapServerException_ReturnsMessage()
    {
        var ex = new MapServerException("Unknown layer: 7");

        Assert.Equal("Unknown layer: 7", ex.ToClientMessage("abc12345"));
    }

    [Fact]
    public void ToClientMessage_DerivedMapServerException_ReturnsMessage()
    {
        var ex = new NotAuthorizedException("Not authorized");

        Assert.True(ex.IsClientMessage);
        Assert.Equal("Not authorized", ex.ToClientMessage("abc12345"));
    }

    [Theory]
    [MemberData(nameof(InternalExceptions))]
    public void ToClientMessage_OtherException_ReturnsGenericTextWithoutInternals(Exception ex)
    {
        string message = ex.ToClientMessage("abc12345");

        Assert.False(ex.IsClientMessage);
        Assert.Equal("internal server error (id: abc12345)", message);
        Assert.DoesNotContain(ex.Message, message);
    }

    public static TheoryData<Exception> InternalExceptions => new()
    {
        new Exception(@"Can't open C:\gview\services\secret.mxl"),
        new NullReferenceException(),
        new InvalidOperationException("Npgsql: password authentication failed for user \"gis\" Host=db01"),
        new ArgumentException("SQL Parser Exception (1=1; drop table x)")
    };

    [Fact]
    public void ToLogMessage_ContainsIdMessageInnerExceptionAndStackTrace()
    {
        Exception ex;
        try
        {
            ThrowNested();
            throw new UnreachableException();
        }
        catch (Exception caught)
        {
            ex = caught;
        }

        string log = ex.ToLogMessage("abc12345");

        Assert.StartsWith("error-id abc12345: ", log);
        Assert.Contains("outer message", log);
        Assert.Contains("inner message", log);
        Assert.Contains(nameof(ThrowNested), log);  // stack trace
    }

    [Fact]
    public void ServiceRequest_ErrorId_IsCreatedOnceAndCanBeSet()
    {
        var request = new ServiceRequest("service", "folder", "");

        string id = request.ErrorId;

        Assert.Matches("^[0-9a-f]{8}$", id);
        Assert.Equal(id, request.ErrorId);

        request.ErrorId = "12345678";
        Assert.Equal("12345678", request.ErrorId);
    }

    private static void ThrowNested()
    {
        try
        {
            throw new InvalidOperationException("inner message");
        }
        catch (Exception inner)
        {
            throw new Exception("outer message", inner);
        }
    }

    private class UnreachableException : Exception { }
}
