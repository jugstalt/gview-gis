using Xunit;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Store;
using gView.Framework.NetworkModel.Types;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Tests;

public class TypeCatalogTests
{
    private static List<FacetConfig> Facets() =>
    [
        new FacetConfig { Name = "Ebene", Values = new() { [1] = "Niederspannung" } },
        new FacetConfig { Name = "Objektart" },
    ];

    private static NetworkMetadata Metadata(TypeCatalog catalog) => new()
    {
        Facets = catalog.FacetMetadata().ToList(),
        Types = catalog.TypeMetadata().ToList(),
    };

    [Fact]
    public void Resolve_SameTexts_GiveSameTypeIgnoringCase()
    {
        var catalog = new TypeCatalog(Facets(), null);

        int a = catalog.Resolve(ElementKind.Node, ["Niederspannung", "Kasten"]);
        int b = catalog.Resolve(ElementKind.Node, ["niederspannung", " kasten "]);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Resolve_NodeAndEdgeWithSameTexts_AreDifferentTypes()
    {
        var catalog = new TypeCatalog(Facets(), null);

        Assert.NotEqual(
            catalog.Resolve(ElementKind.Node, ["Niederspannung", "X"]),
            catalog.Resolve(ElementKind.Edge, ["Niederspannung", "X"]));
    }

    [Fact]
    public void Resolve_FixedValues_WinAndNewValuesAreNumberedAfterThem()
    {
        var catalog = new TypeCatalog(Facets(), null);

        catalog.Resolve(ElementKind.Node, ["Mittelspannung", "Trafo"]);

        var ebene = catalog.FacetMetadata().First().Values;
        Assert.Equal("Niederspannung", ebene[1]);
        Assert.Equal("Mittelspannung", ebene[2]);
    }

    [Fact]
    public void Resolve_WithPreviousBuild_KeepsNumbersAndTypeIdsInAnyOrder()
    {
        var first = new TypeCatalog(Facets(), null);
        int kasten = first.Resolve(ElementKind.Node, ["Niederspannung", "Kasten"]);
        int kunde = first.Resolve(ElementKind.Node, ["Niederspannung", "Kunde"]);
        var previous = Metadata(first);

        var second = new TypeCatalog(Facets(), previous);
        int newType = second.Resolve(ElementKind.Node, ["Niederspannung", "Muffe"]);
        int kunde2 = second.Resolve(ElementKind.Node, ["Niederspannung", "Kunde"]);

        Assert.Equal(kunde, kunde2);
        Assert.True(newType > Math.Max(kasten, kunde));
        var kastenMetadata = second.TypeMetadata().Single(t => t.Id == kasten);
        Assert.False(kastenMetadata.Used);
        Assert.Equal("Niederspannung.Kasten", kastenMetadata.Name);
    }

    [Fact]
    public void ToRegistry_ResolvesPatterns()
    {
        var catalog = new TypeCatalog(Facets(), null);
        int kasten = catalog.Resolve(ElementKind.Node, ["Niederspannung", "Kasten"]);
        int kunde = catalog.Resolve(ElementKind.Node, ["Niederspannung", "Kunde"]);

        var kastenOnly = catalog.ToRegistry().Nodes("[*,Kasten]");
        var both = catalog.ToRegistry().Nodes("[*,Kasten]|[*,Kunde]");

        Assert.True(kastenOnly.Contains(kasten));
        Assert.False(kastenOnly.Contains(kunde));
        Assert.True(both.Contains(kasten) && both.Contains(kunde));
    }
}
