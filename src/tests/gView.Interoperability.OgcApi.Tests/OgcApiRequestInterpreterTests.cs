using gView.Framework.Common;
using gView.Framework.Core.MapServer;
using gView.Interoperability.OgcApi.Request;

namespace gView.Interoperability.OgcApi.Tests;

public class OgcApiRequestInterpreterTests
{
    private static async Task<AccessTypes> RequiredAccessTypes(string operation)
    {
        var interpreter = new OgcApiRequestInterpreter();
        var request = new ServiceRequest("service", "folder", "") { Method = operation };
        var context = await ServiceRequestContext.TryCreate(null!, interpreter, request, checkSecurity: false);

        return interpreter.RequiredAccessTypes(context);
    }

    [Theory]
    [InlineData(OgcApiOperations.Items)]
    [InlineData(OgcApiOperations.Item)]
    public async Task RequiredAccessTypes_FeatureRequests_RequireQuery(string operation)
    {
        Assert.Equal(AccessTypes.Query, await RequiredAccessTypes(operation));
    }

    [Theory]
    [InlineData(OgcApiOperations.CreateItem)]
    [InlineData(OgcApiOperations.ReplaceItem)]
    [InlineData(OgcApiOperations.UpdateItem)]
    [InlineData(OgcApiOperations.DeleteItem)]
    public async Task RequiredAccessTypes_EditRequests_RequireEdit(string operation)
    {
        Assert.Equal(AccessTypes.Edit, await RequiredAccessTypes(operation));
    }

    [Fact]
    public async Task RequiredAccessTypes_MapRequests_RequireMap()
    {
        Assert.Equal(AccessTypes.Map, await RequiredAccessTypes(OgcApiOperations.Map));
    }

    [Theory]
    [InlineData(OgcApiOperations.LandingPage)]
    [InlineData(OgcApiOperations.Conformance)]
    [InlineData(OgcApiOperations.Api)]
    [InlineData(OgcApiOperations.Collections)]
    [InlineData(OgcApiOperations.Collection)]
    [InlineData(OgcApiOperations.Queryables)]
    [InlineData(OgcApiOperations.Options)]
    public async Task RequiredAccessTypes_MetadataRequests_RequireInterfaceOnly(string operation)
    {
        Assert.Equal(AccessTypes.None, await RequiredAccessTypes(operation));
    }

    [Fact]
    public void IdentityName_IsOgcApi()
    {
        Assert.Equal("ogcapi", new OgcApiRequestInterpreter().IdentityName);
    }
}
