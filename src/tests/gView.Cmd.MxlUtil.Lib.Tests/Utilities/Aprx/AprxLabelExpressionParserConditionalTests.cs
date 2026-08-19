using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxLabelExpressionParser"/>'s <c>Function ... If/ElseIf/[Else] ... End
/// Function</c> handling: every generated conditional script is round-tripped through the real
/// <c>SimpleScriptInterpreter</c> (via <see cref="ScriptSimulator"/>) for every field-value
/// combination that matters, rather than just asserting the generated script text - that's what
/// actually proves the conversion is behaviourally correct, not merely "looks plausible".
/// </summary>
public class AprxLabelExpressionParserConditionalTests
{
    private static AprxLabelExpressionParser.ConversionResult Convert(string source)
    {
        var ok = AprxLabelExpressionParser.TryConvert(source, out var result);
        Assert.True(ok, $"Expected the expression to convert successfully, but it was rejected:\n{source}");
        return result!;
    }

    // -----------------------------------------------------------------------
    // If/ElseIf/no-Else with two optional fields, both appearing as whole
    // optional lines (each guarded by one field being present)
    // -----------------------------------------------------------------------

    private const string StptkNrBaujahrSource = """
        Function FindLabel ( [STPKT_NR], [NORMBEZEICHNUNG], [BAUJAHR] )
        if [STPKT_NR] <> "" and [BAUJAHR] <> "" then
         FindLabel = "Nr " & [STPKT_NR] &vbnewline& [NORMBEZEICHNUNG] & vbnewline & "BJ "& [BAUJAHR]
        elseif [BAUJAHR] <> "" then
         FindLabel = [NORMBEZEICHNUNG] & vbnewline & "BJ "& [BAUJAHR]
        elseif [STPKT_NR] <> "" then
         FindLabel = "Nr " & [STPKT_NR] &vbnewline& [NORMBEZEICHNUNG]
        end if
        End Function
        """;

    [Fact]
    public void StptkNrBaujahr_IsConditional()
    {
        Assert.True(Convert(StptkNrBaujahrSource).IsConditional);
    }

    [Theory]
    [InlineData("12", "1998", "Nr 12\nSchacht\nBJ 1998")]
    [InlineData("", "1998", "Schacht\nBJ 1998")]
    [InlineData("12", "", "Nr 12\nSchacht")]
    [InlineData("", "", "")] // no "Else" in source -> VB (and the conversion) returns empty
    public void StptkNrBaujahr_MatchesOriginalVbSemantics(string stpktNr, string baujahr, string expected)
    {
        var script = Convert(StptkNrBaujahrSource).Expression;

        var actual = ScriptSimulator.Evaluate(
            script,
            ("STPKT_NR", stpktNr),
            ("NORMBEZEICHNUNG", "Schacht"),
            ("BAUJAHR", baujahr));

        Assert.Equal(expected.Replace("\n", Environment.NewLine), actual);
    }

    // -----------------------------------------------------------------------
    // If/ElseIf/no-Else where the SAME line's text differs between branches
    // (a leading "Nr " only appears in the branch without the other field) -
    // this needs the per-branch (not per-line) guard reconstruction.
    // -----------------------------------------------------------------------

    private const string EquipmentConditionalPrefixSource = """
        Function FindLabel ( [EQUIPMENT],[SAP_NORMBEZ],  [SAP_BAUJJ] )
        if [EQUIPMENT] <> "" and [SAP_BAUJJ] <> "" then
         FindLabel =  [EQUIPMENT] &vbnewline& [SAP_NORMBEZ] & vbnewline & "BJ "& [SAP_BAUJJ]
        elseif [SAP_BAUJJ] <> "" then
         FindLabel = [SAP_NORMBEZ]  & vbnewline & "BJ "& [SAP_BAUJJ]
        elseif [EQUIPMENT] <> "" then
         FindLabel = "Nr " & [EQUIPMENT] &vbnewline& [SAP_NORMBEZ]
        end if
        End Function
        """;

    [Theory]
    [InlineData("4711", "2001", "4711\nSchieber\nBJ 2001")]  // both present -> no "Nr " prefix
    [InlineData("", "2001", "Schieber\nBJ 2001")]
    [InlineData("4711", "", "Nr 4711\nSchieber")]             // only EQUIPMENT -> "Nr " prefix
    [InlineData("", "", "")]
    public void EquipmentConditionalPrefix_MatchesOriginalVbSemantics(string equipment, string baujj, string expected)
    {
        var script = Convert(EquipmentConditionalPrefixSource).Expression;

        var actual = ScriptSimulator.Evaluate(
            script,
            ("EQUIPMENT", equipment),
            ("SAP_NORMBEZ", "Schieber"),
            ("SAP_BAUJJ", baujj));

        Assert.Equal(expected.Replace("\n", Environment.NewLine), actual);
    }

    // -----------------------------------------------------------------------
    // Value-equality / not-equality conditions ([Field] = "Value", <> "Value")
    // -----------------------------------------------------------------------

    private const string ValueEqualitySource = """
        Function FindLabel ( [TYP], [NAME] )
        if [TYP] = "Schieber" then
         FindLabel = "S: " & [NAME]
        elseif [TYP] = "Ventil" then
         FindLabel = "V: " & [NAME]
        else
         FindLabel = [NAME]
        end if
        End Function
        """;

    [Theory]
    [InlineData("Schieber", "Ost", "S: Ost")]
    [InlineData("Ventil", "West", "V: West")]
    [InlineData("Hydrant", "Nord", "Nord")] // matches neither -> falls through to the "Else" branch
    [InlineData("", "Sued", "Sued")]
    public void ValueEquality_MatchesOriginalVbSemantics(string typ, string name, string expected)
    {
        var script = Convert(ValueEqualitySource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("TYP", typ), ("NAME", name));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NotEqualToValue_SingleIfWithNoElse_RespectsCondition()
    {
        // Regression test: a Function with exactly one "If ... Then" and no "Else" was, before
        // the fix, wrongly treated as unconditional (ParseIfElseBranches produces exactly one
        // branch either way, and the old code assumed "one branch" always meant "no condition
        // at all") - the condition itself was silently dropped and the text always shown.
        const string source = """
            Function FindLabel ( [STATUS], [NAME] )
            if [STATUS] <> "Ausser Betrieb" then
             FindLabel = [NAME]
            end if
            End Function
            """;

        var script = Convert(source).Expression;
        Assert.True(AprxLabelExpressionParser.TryConvert(source, out var result));
        Assert.True(result!.IsConditional, "A single If without Else must still be treated as conditional.");

        Assert.Equal("Pumpe1", ScriptSimulator.Evaluate(script, ("STATUS", "Aktiv"), ("NAME", "Pumpe1")));
        Assert.Equal("", ScriptSimulator.Evaluate(script, ("STATUS", "Ausser Betrieb"), ("NAME", "Pumpe1")));
    }

    // -----------------------------------------------------------------------
    // "and (... or ... or ...)" conditions, distributed into AND-only sibling
    // branches, combined with ArcGIS Pro <CLR> rich-text tag stripping
    // -----------------------------------------------------------------------

    private const string OrGroupWithClrTagsSource = """
        Function FindLabel ([SA4],[SA1],[SA2],[GDR],[SA3])

        If [SA1] ="N" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='255' green='0' blue='0'>" &[SA4]&"</CLR>"

        ElseIf [SA1] ="M" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='56' green='168' blue='0'>" &[SA4]&"</CLR>"

        ElseIf [SA1] ="H" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='0' green='0' blue='255'>" &[SA4]&"</CLR>"

        ElseIf [SA1] ="F" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='255' green='0' blue='197'>" &[SA4]&"</CLR>"

        ElseIf [SA1] ="N" AND ([SA2] ="HL" OR [SA2]="SL" OR [SA2] ="VL") Then
          FindLabel = "<CLR red='255' green='0' blue='0'>" &[SA4]&[GDR]&" "&[SA3]&"</CLR>"

        ElseIf [SA1] ="M" AND ([SA2] ="HL" OR [SA2]="SL" OR [SA2] ="VL") Then
          FindLabel = "<CLR red='56' green='168' blue='0'>" &[SA4]&[GDR]&" "&[SA3]&"</CLR>"

        ElseIf [SA1] ="H" AND ([SA2] ="HL" OR [SA2]="SL" OR [SA2] ="VL") Then
          FindLabel = "<CLR red='0' green='0' blue='255'>" &[SA4]&[GDR]&" "&[SA3]&"</CLR>"

        ElseIf [SA1] ="F" AND ([SA2] ="HL" OR [SA2]="SL" OR [SA2] ="VL") Then
          FindLabel = "<CLR red='255' green='0' blue='197'>" &[SA4]&[GDR]&" "&[SA3]&"</CLR>"
        End If
        End Function
        """;

    [Fact]
    public void OrGroupWithClrTags_GeneratedScript_ContainsNoClrTags()
    {
        var script = Convert(OrGroupWithClrTagsSource).Expression;

        Assert.DoesNotContain("<CLR", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("</CLR", script, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("N", "HA", "SA4x")]
    [InlineData("M", "HA", "SA4x")]
    [InlineData("H", "HA", "SA4x")]
    [InlineData("F", "HA", "SA4x")]
    [InlineData("N", "HL", "SA4xGDRx SA3x")]
    [InlineData("M", "SL", "SA4xGDRx SA3x")]
    [InlineData("H", "VL", "SA4xGDRx SA3x")]
    [InlineData("F", "HL", "SA4xGDRx SA3x")]
    [InlineData("X", "HA", "")]  // SA1 matches nothing -> no branch fires -> empty (no Else)
    [InlineData("N", "ZZ", "")]  // SA2 matches nothing -> no branch fires -> empty (no Else)
    public void OrGroupWithClrTags_MatchesOriginalVbSemantics(string sa1, string sa2, string expected)
    {
        var script = Convert(OrGroupWithClrTagsSource).Expression;

        var actual = ScriptSimulator.Evaluate(
            script,
            ("SA4", "SA4x"),
            ("SA1", sa1),
            ("SA2", sa2),
            ("GDR", "GDRx"),
            ("SA3", "SA3x"));

        Assert.Equal(expected, actual);
    }

    // -----------------------------------------------------------------------
    // Chained Replace(...) calls -> gView's existing "@@replace(search,replacement)" command
    // -----------------------------------------------------------------------

    private const string ReplaceChainSource = """
        Function FindLabel ( [TYP] )
          Beschriftung= Replace ([TYP],"Hausanschluss","HA")
          Beschriftung= Replace (Beschriftung,"Sonstiger Endpunkt","")
          Beschriftung= Replace (Beschriftung,"Sonstiger Punkt","")
          Beschriftung= Replace (Beschriftung,"Sonstiges Punktobjekt","")
          Beschriftung= Replace (Beschriftung,"Reserve","Res.")
        FindLabel = Beschriftung
        End Function
        """;

    [Fact]
    public void ReplaceChain_IsConditional()
    {
        // "Conditional" here just means "uses the @@ mini-script", not that it branches.
        Assert.True(Convert(ReplaceChainSource).IsConditional);
    }

    [Theory]
    [InlineData("Hausanschluss", "HA")]
    [InlineData("Sonstiger Endpunkt", "")]
    [InlineData("Sonstiger Punkt", "")]
    [InlineData("Sonstiges Punktobjekt", "")]
    [InlineData("Reserve", "Res.")]
    [InlineData("Schacht", "Schacht")] // no rule matches -> unchanged
    public void ReplaceChain_AppliesEachRuleInOrder(string typ, string expected)
    {
        var script = Convert(ReplaceChainSource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("TYP", typ));

        Assert.Equal(expected, actual);
    }
}
