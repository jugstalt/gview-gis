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

    // -----------------------------------------------------------------------
    // Equality/inequality against an unquoted number (reuses the same Equals/NotEquals
    // predicate kinds as the quoted-string form - a plain string compare against the number's
    // own text is enough for equality purposes).
    // -----------------------------------------------------------------------

    [Fact]
    public void UnquotedNumericEquals_SingleIfNoElse_RespectsCondition()
    {
        const string source = """
            Function F([A])
            if [A] = 5 then
             F = "yes"
            end if
            End Function
            """;
        var script = Convert(source).Expression;

        Assert.Equal("yes", ScriptSimulator.Evaluate(script, ("A", "5")));
        Assert.Equal("", ScriptSimulator.Evaluate(script, ("A", "6")));
        Assert.Equal("", ScriptSimulator.Evaluate(script, ("A", "")));
    }

    [Fact]
    public void UnquotedNumericNotEquals_SingleIfNoElse_RespectsCondition()
    {
        const string source = """
            Function F([A])
            if [A] <> 5 then
             F = "yes"
            end if
            End Function
            """;
        var script = Convert(source).Expression;

        Assert.Equal("", ScriptSimulator.Evaluate(script, ("A", "5")));
        Assert.Equal("yes", ScriptSimulator.Evaluate(script, ("A", "6")));
    }

    // -----------------------------------------------------------------------
    // Numeric range comparisons (<, <=, >, >=), bucketing a value into consecutive,
    // non-overlapping ranges - combined with round(...) and <CLR> tag stripping.
    // -----------------------------------------------------------------------

    private const string NumericRangeSource = """
        Function FindLabel (  [NETWORK_KV_DIST]  )
        If [NETWORK_KV_DIST] <= 100 Then
          FindLabel = "<CLR red='56' green='168' blue='0'>" & round([NETWORK_KV_DIST],0)&"m" &"</CLR>"
        ElseIf [NETWORK_KV_DIST] > 100 And [NETWORK_KV_DIST] <= 150 Then
          FindLabel = "<CLR red='94' green='189' blue='0'>" & round([NETWORK_KV_DIST],0)&"m" &"</CLR>"
        ElseIf [NETWORK_KV_DIST] > 150 And [NETWORK_KV_DIST] <= 200 Then
          FindLabel = "<CLR red='255' green='255' blue='0'>" & round([NETWORK_KV_DIST],0)&"m" &"</CLR>"
        ElseIf [NETWORK_KV_DIST] > 200 And [NETWORK_KV_DIST] <= 250 Then
          FindLabel = "<CLR red='255' green='153' blue='0'>" & round([NETWORK_KV_DIST],0)&"m" &"</CLR>"
        ElseIf [NETWORK_KV_DIST] > 250 And [NETWORK_KV_DIST] <= 300 Then
          FindLabel = "<CLR red='255' green='102' blue='153'>" & round([NETWORK_KV_DIST],0)&"m" &"</CLR>"
        ElseIf [NETWORK_KV_DIST] > 300 Then
          FindLabel = "<CLR red='230' green='0' blue='0'>" & round([NETWORK_KV_DIST],0)&"m" &"</CLR>"

        End If
        End Function
        """;

    [Fact]
    public void NumericRange_GeneratedScript_ContainsNoClrTagsAndNoRedundantGuards()
    {
        var script = Convert(NumericRangeSource).Expression;

        Assert.DoesNotContain("<CLR", script, StringComparison.OrdinalIgnoreCase);
        // Consecutive ranges are inherently non-overlapping, so an earlier branch's own
        // conditions should always directly contradict the current branch's - meaning no
        // "exclude" guards should be needed for any branch at all (see Predicate.Contradicts).
        // The only "@@if(...)" guards present should be each branch's own conditions: 1 + 2 + 2
        // + 2 + 2 + 1 = 10.
        Assert.Equal(10, System.Text.RegularExpressions.Regex.Matches(script, "@@if").Count);
    }

    [Theory]
    [InlineData(50, "50m")]     // <= 100
    [InlineData(100, "100m")]   // boundary: <= 100
    [InlineData(125, "125m")]   // > 100 and <= 150
    [InlineData(150, "150m")]   // boundary: > 100 and <= 150
    [InlineData(175, "175m")]   // > 150 and <= 200
    [InlineData(200, "200m")]   // boundary: > 150 and <= 200
    [InlineData(225, "225m")]   // > 200 and <= 250
    [InlineData(250, "250m")]   // boundary: > 200 and <= 250
    [InlineData(275, "275m")]   // > 250 and <= 300
    [InlineData(300, "300m")]   // boundary: > 250 and <= 300
    [InlineData(400, "400m")]   // > 300
    public void NumericRange_MatchesOriginalVbSemantics(double distance, string expected)
    {
        var script = Convert(NumericRangeSource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("NETWORK_KV_DIST", distance.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NumericRange_GermanLocaleDecimalCommaInFieldValue_StillComparesCorrectly()
    {
        // A numeric field's own ToString() uses the current culture (e.g. "125,4" instead of
        // "125.4" under a German locale) - the comma must not break the "value,op,threshold"
        // argument parsing in SimpleScriptInterpreter.
        var script = Convert(NumericRangeSource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("NETWORK_KV_DIST", "125,4"));

        Assert.Equal("125m", actual);
    }

    [Fact]
    public void NumericRange_EmptyField_NoBranchMatches_ProducesEmptyLabel()
    {
        var script = Convert(NumericRangeSource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("NETWORK_KV_DIST", ""));

        Assert.Equal("", actual);
    }

    [Fact]
    public void NumericRange_ReversedOperandOrder_IsAlsoSupported()
    {
        // "100 >= [Field]" means "[Field] <= 100" - the operator flips.
        var source = """
            Function F([A])
            If 100 >= [A] Then
             F = "low"
            ElseIf [A] > 100 Then
             F = "high"
            End If
            End Function
            """;
        var script = Convert(source).Expression;

        Assert.Equal("low", ScriptSimulator.Evaluate(script, ("A", "100")));
        Assert.Equal("high", ScriptSimulator.Evaluate(script, ("A", "101")));
    }

    // -----------------------------------------------------------------------
    // Numeric equality/inequality against an unquoted number ("[Field] = 200"), redundant
    // parentheses around a whole AND-only condition, and AND-before-OR precedence
    // -----------------------------------------------------------------------

    private const string PnrTypSichPnrSource = """
        Function FindLabel ( [PNR], [OEK], [TYP], [SICH_PNR] )
          If ( [TYP] = "TP" And [SICH_PNR] = 1) Then
            FindLabel = [PNR] & "-" & [OEK]
          ElseIf ( [TYP] = "EP" And [SICH_PNR] = 1) Then
            FindLabel = "<UND>" & [PNR] & "</UND>"
          ElseIf ( [TYP] = "HP" And [SICH_PNR] = 1) Then
            FindLabel = [PNR]
          ElseIf ( [TYP] = "PP" And [SICH_PNR] = 1) Then
            FindLabel = [PNR]
          End if
        End Function
        """;

    [Fact]
    public void RedundantOuterParens_AroundWholeAndCondition_IsUnwrappedCorrectly()
    {
        // "( [TYP] = "TP" And [SICH_PNR] = 1)" wraps an ordinary AND-only condition in one
        // redundant pair of parens - must not be mistaken for an "or"-group.
        Convert(PnrTypSichPnrSource);
    }

    [Fact]
    public void UnquotedNumericEquality_NoClrOrUndTags_ProducesNoTags()
    {
        var script = Convert(PnrTypSichPnrSource).Expression;

        Assert.DoesNotContain("<UND", script, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("TP", "1", "12-OK1")]
    [InlineData("EP", "1", "12")] // <UND> tag stripped, no branching side effect from it
    [InlineData("HP", "1", "12")]
    [InlineData("PP", "1", "12")]
    [InlineData("TP", "0", "")]   // SICH_PNR != 1 -> no branch matches
    [InlineData("XX", "1", "")]   // TYP matches nothing -> no branch matches
    public void PnrTypSichPnr_MatchesOriginalVbSemantics(string typ, string sichPnr, string expected)
    {
        var script = Convert(PnrTypSichPnrSource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("PNR", "12"), ("OEK", "OK1"), ("TYP", typ), ("SICH_PNR", sichPnr));

        Assert.Equal(expected, actual);
    }

    private const string OrGroupAndNumericEqualitySource = """
        Function FindLabel ( [PNR], [TYP], [SICH_PNR] )
          If ( ([TYP] = "20" Or [TYP] = "24" Or [TYP] = "25") And [SICH_PNR] = 1) Then
            FindLabel = [PNR]
          ElseIf ( ([TYP] = "220" Or [TYP] = "224" Or [TYP] = "225") And [SICH_PNR] = 1) Then
            FindLabel = [PNR]
          End If
        End Function
        """;

    [Theory]
    [InlineData("20", "1", "P1")]
    [InlineData("24", "1", "P1")]
    [InlineData("25", "1", "P1")]
    [InlineData("220", "1", "P1")]
    [InlineData("224", "1", "P1")]
    [InlineData("225", "1", "P1")]
    [InlineData("20", "0", "")]  // SICH_PNR != 1
    [InlineData("99", "1", "")]  // TYP matches neither OR-group
    public void OrGroupAndNumericEquality_MatchesOriginalVbSemantics(string typ, string sichPnr, string expected)
    {
        var script = Convert(OrGroupAndNumericEqualitySource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("PNR", "P1"), ("TYP", typ), ("SICH_PNR", sichPnr));

        Assert.Equal(expected, actual);
    }

    private const string FontSizeByTextgrSource = """
        Function FindLabel ( [TYP], [TEXT], [TEXTGR] )
          If ([TYP] = 200 And [TEXTGR] = 2) Then
            FindLabel = "<FNT size = '8'>" & [TEXT] & "</FNT>"
          ElseIf ([TYP] = 200 And [TEXTGR] = 3) Then
            FindLabel = "<FNT size = '12'>" & [TEXT] & "</FNT>"
          ElseIf ([TYP] = 200 And [TEXTGR] = 20) Then
            FindLabel = "<FNT size = '100'>" & [TEXT] & "</FNT>"
          ElseIf ([TYP] = 130) Then
            FindLabel = "<CLR blue = '255'>" & [TEXT] & "</CLR>"
          ElseIf ([TYP] = 300) Then
            FindLabel = "<CLR red = '255'>" & [TEXT] & "</CLR>"
          End if
        End Function
        """;

    [Fact]
    public void FontSizeByTextgr_NoFntOrClrTags()
    {
        var script = Convert(FontSizeByTextgrSource).Expression;

        Assert.DoesNotContain("<FNT", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<CLR", script, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("200", "2", "hi")]
    [InlineData("200", "20", "hi")]
    [InlineData("130", "999", "hi")] // TYP=130 branch has no TEXTGR condition at all
    [InlineData("300", "999", "hi")]
    [InlineData("999", "999", "")]   // matches nothing
    public void FontSizeByTextgr_MatchesOriginalVbSemantics(string typ, string textgr, string expected)
    {
        var script = Convert(FontSizeByTextgrSource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("TYP", typ), ("TEXT", "hi"), ("TEXTGR", textgr));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AndBeforeOrPrecedence_UnparenthesizedMix_ResolvesWhenBranchesDontOverlap()
    {
        // "A and B or A and C" (no parens) parses per standard VB precedence as
        // "(A and B) or (A and C)" - and here the two DNF branches auto-contradict on B vs C,
        // so no extra guard is needed and this converts cleanly (contrast with
        // AprxLabelExpressionParserRejectionTests.OrOfCrossFieldAndGroup_IsRejected, where the
        // disjuncts don't share a contradicting field and it's correctly rejected instead).
        var source = """
            Function F([A],[B],[C])
            if [A] = "X" and [B] = "Y" or [A] = "X" and [B] = "Z" then
             F = [A]
            end if
            End Function
            """;
        var script = Convert(source).Expression;

        Assert.Equal("X", ScriptSimulator.Evaluate(script, ("A", "X"), ("B", "Y")));
        Assert.Equal("X", ScriptSimulator.Evaluate(script, ("A", "X"), ("B", "Z")));
        Assert.Equal("", ScriptSimulator.Evaluate(script, ("A", "X"), ("B", "other")));
        Assert.Equal("", ScriptSimulator.Evaluate(script, ("A", "notX"), ("B", "Y")));
    }

    // -----------------------------------------------------------------------
    // IsNull(...) / Not IsNull(...), and "=="/"!=" as synonyms for "="/"<>"
    // -----------------------------------------------------------------------

    [Fact]
    public void IsNull_MatchesOriginalVbSemantics()
    {
        const string source = """
            Function FindLabel ( [VERTRAGSLEISTUNG_KW],[BER_P])
             if IsNull([VERTRAGSLEISTUNG_KW]) then
              FindLabel = [BER_P] & " kW"
             else
              FindLabel = [VERTRAGSLEISTUNG_KW] & " kW"
             end if
            End Function
            """;
        var script = Convert(source).Expression;

        Assert.Equal("5 kW", ScriptSimulator.Evaluate(script, ("VERTRAGSLEISTUNG_KW", ""), ("BER_P", "5")));
        Assert.Equal("10 kW", ScriptSimulator.Evaluate(script, ("VERTRAGSLEISTUNG_KW", "10"), ("BER_P", "5")));
    }

    [Fact]
    public void NotIsNull_MatchesOriginalVbSemantics()
    {
        const string source = """
            Function F([A])
            if Not IsNull([A]) then
             F = "present"
            end if
            End Function
            """;
        var script = Convert(source).Expression;

        Assert.Equal("present", ScriptSimulator.Evaluate(script, ("A", "x")));
        Assert.Equal("", ScriptSimulator.Evaluate(script, ("A", "")));
    }

    [Theory]
    [InlineData("==", "Schieber", "S")]  // [TYP] == "Schieber" holds -> "if" branch
    [InlineData("==", "Ventil", "V")]    // doesn't hold -> "else" branch
    [InlineData("!=", "Ventil", "S")]    // [TYP] != "Schieber" holds -> "if" branch
    [InlineData("!=", "Schieber", "V")]  // doesn't hold -> "else" branch
    public void PythonStyleEqualityOperators_AreSynonymsForVbOperators(string op, string typ, string expected)
    {
        var source = $$"""
            Function F([TYP])
            if [TYP] {{op}} "Schieber" then
             F = "S"
            else
             F = "V"
            end if
            End Function
            """;
        var script = Convert(source).Expression;

        Assert.Equal(expected, ScriptSimulator.Evaluate(script, ("TYP", typ)));
    }

    // -----------------------------------------------------------------------
    // Sequential conditional-append chain: a base assignment followed by several
    // independent "if COND then NAME = NAME & ... end if" blocks (not an If/ElseIf/Else
    // chain - zero, one, or several can fire together for the same feature).
    // -----------------------------------------------------------------------

    private const string AppendChainSource = """
        Function FindLabel ([KENNUNG], [NAME], [CDMA], [DMR], [DAFU], [RIFU])

                FindLabel = [KENNUNG] & " " & [NAME]

                if [CDMA] = "Ja" then
                        FindLabel = FindLabel &  vbnewline & "    CDMA"
                end if

                if [DMR] = "Ja" then
                        FindLabel = FindLabel &  vbnewline & "    DMR"
                end if

                if [DAFU] = "Ja" then
                        FindLabel = FindLabel &  vbnewline & "    DAFU"
                end if

                if [RIFU] = "Ja" then
                        FindLabel = FindLabel &  vbnewline & "    RIFU"
                end if

        End Function
        """;

    [Fact]
    public void AppendChain_IsConditional()
    {
        Assert.True(Convert(AppendChainSource).IsConditional);
    }

    [Theory]
    [InlineData("", "", "", "", "AB1 Name1")]
    [InlineData("Ja", "", "", "", "AB1 Name1\n    CDMA")]
    [InlineData("Ja", "Ja", "", "", "AB1 Name1\n    CDMA\n    DMR")]
    [InlineData("", "Ja", "", "", "AB1 Name1\n    DMR")] // skipping an earlier flag works too
    [InlineData("Ja", "Ja", "Ja", "Ja", "AB1 Name1\n    CDMA\n    DMR\n    DAFU\n    RIFU")]
    public void AppendChain_MatchesOriginalVbSemantics(string cdma, string dmr, string dafu, string rifu, string expected)
    {
        var script = Convert(AppendChainSource).Expression;

        var actual = ScriptSimulator.Evaluate(
            script,
            ("KENNUNG", "AB1"), ("NAME", "Name1"),
            ("CDMA", cdma), ("DMR", dmr), ("DAFU", dafu), ("RIFU", rifu));

        Assert.Equal(expected.Replace("\n", Environment.NewLine), actual);
    }
}
