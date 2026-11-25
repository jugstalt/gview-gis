using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using gView.Interoperability.Extensions.Cql2;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace gView.Server.Tests;

public class QueryCollectionExtensionsTests
{
    private static GetFeaturesRequest Request(string queryString)
        => new QueryCollection(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString)).ToGetFeaturesRequest();

    [Fact]
    public void Defaults_AllFieldsWithoutGeometry()
    {
        var request = Request("");

        Assert.Equal(["*"], request.OutFields);
        Assert.Equal(QueryCommand.Select, request.Command);
        Assert.Equal(GeometryResult.None, request.ReturnGeometry);
        Assert.Null(request.Filter);
        Assert.Null(request.SpatialFilter);
    }

    [Fact]
    public void ListsCrsAndPaging()
    {
        var request = Request("?outFields=NAME, EW&returnGeometry=geometry&outCRS=4326&orderBy=NAME DESC&objectIds=1,2&limit=10&offset=20&command=idsOnly");

        Assert.Equal(["NAME", "EW"], request.OutFields);
        Assert.Equal(GeometryResult.Geometry, request.ReturnGeometry);
        Assert.Equal("EPSG:4326", request.OutCRS!.Properties["name"]);
        Assert.Equal(["NAME DESC"], request.OrderByFields);
        Assert.Equal(["1", "2"], request.ObjectIds);
        Assert.Equal(10, request.Limit);
        Assert.Equal(20, request.Offset);
        Assert.Equal(QueryCommand.IdsOnly, request.Command);
    }

    [Fact]
    public void BBox_IsASpatialFilter()
    {
        var request = Request("?bbox=1,2,3,4&bbox-crs=EPSG:31256&spatialOperator=within");

        Assert.Equal(1, request.SpatialFilter!.BBox!.MinX);
        Assert.Equal(4, request.SpatialFilter.BBox.MaxY);
        Assert.Equal("EPSG:31256", request.SpatialFilter.CRS!.Properties["name"]);
        Assert.Equal(SpatialOperator.Within, request.SpatialFilter.Operator);
        Assert.Null(request.SpatialFilter.Geometry);
    }

    [Fact]
    public void WhereAndCql2Text()
    {
        var request = Request("?where=EW>1000&filter=NAME='Wien'");

        Assert.Equal("EW>1000", request.Filter!.WhereClause);
        Assert.Equal(CqlTextParser.Parse("NAME='Wien'"), request.Filter.Cql2Predicate());
    }

    [Fact]
    public void Cql2Json()
    {
        var json = """{"op":"=","args":[{"property":"NAME"},"Wien"]}""";
        var request = Request($"?filter-lang=cql2-json&filter={Uri.EscapeDataString(json)}");

        Assert.IsType<JsonElement>(request.Filter!.Cql2);
        Assert.Equal(CqlTextParser.Parse("NAME='Wien'"), request.Filter.Cql2Predicate());
    }

    [Theory]
    [InlineData("?limit=abc")]
    [InlineData("?limit=-1")]
    [InlineData("?bbox=1,2,3")]
    [InlineData("?outCRS=wgs84")]
    [InlineData("?returnGeometry=all")]
    [InlineData("?filter=x&filter-lang=sql")]
    [InlineData("?filter={&filter-lang=cql2-json")]
    public void Invalid_IsBadRequest(string queryString)
    {
        Assert.Throws<MapServerException>(() => Request(queryString));
    }
}
