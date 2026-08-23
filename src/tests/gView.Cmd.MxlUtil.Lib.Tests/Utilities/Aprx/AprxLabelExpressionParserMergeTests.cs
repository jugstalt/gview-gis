using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for the branch-merging readability pass (<c>CoalesceForRendering</c>, wired into
/// <c>TryBuildConditionalScript</c>): several ElseIf branches that agree on everything except one
/// field's specific Equals value collapse into a single branch using the variable-arg
/// <c>@@if([Field],in,V1,V2,...)</c> form, instead of emitting one near-duplicate "@@if" block per
/// value. As with every other conditional conversion, correctness is what's actually asserted -
/// every test round-trips the generated script through the real <c>SimpleScriptInterpreter</c> (via
/// <see cref="ScriptSimulator"/>) for every combination of field values that matters, not just the
/// generated text. The number of "@@if(" occurrences is also checked, as direct evidence the merge
/// actually happened (rather than just coincidentally producing correct output some other way).
/// </summary>
public class AprxLabelExpressionParserMergeTests
{
    private static AprxLabelExpressionParser.ConversionResult Convert(string source)
    {
        var ok = AprxLabelExpressionParser.TryConvert(source, out var result);
        Assert.True(ok, $"Expected the expression to convert successfully, but it was rejected:\n{source}");
        return result!;
    }

    private static int CountIfs(string script) => script.Split("@@if(").Length - 1;

    // -----------------------------------------------------------------------
    // The user's real-world shape: two fields (SA1, SA2), each independently enumerated across
    // several ElseIf branches, where the output only actually depends on SA2 - reduces from 16
    // branches (4 SA1 values x 4 SA2 values) down to 2.
    // -----------------------------------------------------------------------

    private const string Sa1Sa2Source = """
        Function FindLabel ( [SA1], [SA2], [SA3], [SA4], [GDR], [MATTYP] )
        if [SA1] = "N" and [SA2] = "HA" then
         FindLabel = [SA4] & " " & [MATTYP]
        elseif [SA1] = "N" and [SA2] = "HL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "N" and [SA2] = "SL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "N" and [SA2] = "VL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "M" and [SA2] = "HA" then
         FindLabel = [SA4] & " " & [MATTYP]
        elseif [SA1] = "M" and [SA2] = "HL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "M" and [SA2] = "SL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "M" and [SA2] = "VL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "H" and [SA2] = "HA" then
         FindLabel = [SA4] & " " & [MATTYP]
        elseif [SA1] = "H" and [SA2] = "HL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "H" and [SA2] = "SL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "H" and [SA2] = "VL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "F" and [SA2] = "HA" then
         FindLabel = [SA4] & " " & [MATTYP]
        elseif [SA1] = "F" and [SA2] = "HL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "F" and [SA2] = "SL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        elseif [SA1] = "F" and [SA2] = "VL" then
         FindLabel = [SA4] & [GDR] & " " & [SA3] & " " & [MATTYP]
        end if
        End Function
        """;

    [Fact]
    public void Sa1Sa2_CollapsesFrom16BranchesToTwoIfBlocks()
    {
        var script = Convert(Sa1Sa2Source).Expression;

        // 2 branches x 2 conditions each ("in(SA1,...)" plus the SA2 guard) = 4 "@@if(" total,
        // down from 16 (one per original ElseIf branch).
        Assert.Equal(4, CountIfs(script));
        Assert.Contains(",in,", script);
    }

    [Theory]
    [InlineData("N", "HA", "A B")]
    [InlineData("M", "HA", "A B")]
    [InlineData("H", "HA", "A B")]
    [InlineData("F", "HA", "A B")]
    [InlineData("N", "HL", "AGGG C B")]
    [InlineData("N", "SL", "AGGG C B")]
    [InlineData("N", "VL", "AGGG C B")]
    [InlineData("M", "VL", "AGGG C B")]
    [InlineData("H", "SL", "AGGG C B")]
    [InlineData("F", "HL", "AGGG C B")]
    [InlineData("X", "HA", "")] // SA1 outside the enumerated set -> no branch matches, like VB's implicit empty
    [InlineData("N", "XX", "")] // SA2 outside the enumerated set -> no branch matches
    public void Sa1Sa2_MatchesOriginalVbSemantics(string sa1, string sa2, string expected)
    {
        var script = Convert(Sa1Sa2Source).Expression;

        var actual = ScriptSimulator.Evaluate(
            script,
            ("SA1", sa1),
            ("SA2", sa2),
            ("SA3", "C"),
            ("SA4", "A"),
            ("GDR", "GGG"),
            ("MATTYP", "B"));

        Assert.Equal(expected, actual);
    }

    // -----------------------------------------------------------------------
    // A simpler single-field case: one field enumerated across several branches that all produce
    // the exact same output - collapses to a single merged branch.
    // -----------------------------------------------------------------------

    private const string SingleFieldEnumSource = """
        Function FindLabel ( [TYP] )
        if [TYP] = "Schieber" then
         FindLabel = "S"
        elseif [TYP] = "Ventil" then
         FindLabel = "S"
        elseif [TYP] = "Hydrant" then
         FindLabel = "S"
        end if
        End Function
        """;

    [Fact]
    public void SingleFieldSameOutput_CollapsesToOneIfBlock()
    {
        var script = Convert(SingleFieldEnumSource).Expression;

        Assert.Equal(1, CountIfs(script));
        Assert.Contains(",in,Schieber,Ventil,Hydrant", script);
    }

    [Theory]
    [InlineData("Schieber", "S")]
    [InlineData("Ventil", "S")]
    [InlineData("Hydrant", "S")]
    [InlineData("Sonstiges", "")]
    public void SingleFieldSameOutput_MatchesOriginalVbSemantics(string typ, string expected)
    {
        var script = Convert(SingleFieldEnumSource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("TYP", typ));

        Assert.Equal(expected, actual);
    }

    // -----------------------------------------------------------------------
    // Branches that differ by more than just one field's value must NOT be merged - each output
    // genuinely depends on both fields here, so every branch stays separate.
    // -----------------------------------------------------------------------

    private const string NoMergePossibleSource = """
        Function FindLabel ( [A], [B] )
        if [A] = "X" and [B] = "1" then
         FindLabel = "X1"
        elseif [A] = "Y" and [B] = "2" then
         FindLabel = "Y2"
        end if
        End Function
        """;

    [Fact]
    public void DifferentOutputsPerCombination_StaysUnmerged()
    {
        var script = Convert(NoMergePossibleSource).Expression;

        Assert.Equal(4, CountIfs(script)); // 2 branches x 2 conditions each, nothing collapsible
        Assert.DoesNotContain(",in,", script);
    }

    [Theory]
    [InlineData("X", "1", "X1")]
    [InlineData("Y", "2", "Y2")]
    [InlineData("X", "2", "")]
    [InlineData("Y", "1", "")]
    public void DifferentOutputsPerCombination_MatchesOriginalVbSemantics(string a, string b, string expected)
    {
        var script = Convert(NoMergePossibleSource).Expression;

        var actual = ScriptSimulator.Evaluate(script, ("A", a), ("B", b));

        Assert.Equal(expected, actual);
    }
}
