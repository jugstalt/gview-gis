using Xunit;
using gView.Framework.NetworkModel.Config;
using SpatialNET.Network;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Tests;

public class NetworkRuleConfigTests
{
    [Fact]
    public void Json_LowerCaseEnumsAndNullMax_AreRead()
    {
        var json = """
            { "rules": [ { "id": "r1", "severity": "warning", "subject": "edge", "subjectTypes": "[*,Kabel,*]",
                           "endFilter": "nodeIsNot", "endTypes": "[*,Kunde,*]", "countTypes": "[*,Kabel,*]", "min": 1, "max": null } ],
              "facets": [ { "name": "Objektart" } ] }
            """;

        var config = JsonSerializer.Deserialize<NetworkConfig>(json, NetworkConfig.JsonOptions)!;

        var rule = Assert.Single(config.Rules);
        Assert.Equal(RuleSeverity.Warning, rule.Severity);
        Assert.Equal(RuleSubject.Edge, rule.Subject);
        Assert.Equal(RuleEndFilter.NodeIsNot, rule.EndFilter);
        Assert.Null(rule.Max);
    }

    [Fact]
    public void Json_RoundTrip_KeepsEveryField()
    {
        var config = new NetworkConfig
        {
            Rules = [new NetworkRuleConfig { Id = "r", Description = "d", Severity = RuleSeverity.Warning, Subject = RuleSubject.Edge,
                SubjectTypes = "A|B", EndFilter = RuleEndFilter.NodeIs, EndTypes = "C", CountTypes = "D", Min = 2, Max = 3 }]
        };

        var back = JsonSerializer.Deserialize<NetworkConfig>(config.ToJson(), NetworkConfig.JsonOptions)!;

        Assert.Equal(config.Rules[0].ToDefinition(), back.Rules[0].ToDefinition());
        Assert.Equal("d", back.Rules[0].Description);
    }

    [Fact]
    public void ToDefinition_EmptyEndTypes_BecomesNull()
    {
        var rule = new NetworkRuleConfig { Id = "r", SubjectTypes = "A", CountTypes = "B", EndTypes = " " };

        Assert.Null(rule.ToDefinition().EndTypes);
    }

    [Theory]
    [InlineData("r", "r")]
    [InlineData("", "x")]
    public void Validate_BadRuleIds_Throws(string first, string second)
    {
        var config = new NetworkConfig
        {
            Facets = [new FacetConfig { Name = "Objektart" }],
            Nodes = [new NodeLayerConfig { Layer = "Kasten", Type = ["Kasten"] }],
            Rules = [new NetworkRuleConfig { Id = first }, new NetworkRuleConfig { Id = second == "x" ? first : second }],
        };

        Assert.Throws<InvalidDataException>(config.Validate);
    }
}
