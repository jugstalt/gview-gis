using gView.Framework.Common;

namespace gView.Framework.Tests.Common;

/// <summary>
/// Tests for <see cref="SimpleScriptInterpreter"/>'s "@@start / @@if(...) / @@endif / @@end /
/// @@replace(...)" mini-script, with particular focus on the two things touched while building
/// gView.Cmd.MxlUtil.Lib's ArcGIS Pro label-expression converter:
/// <list type="bullet">
/// <item>proper stack-based nesting of "@@if(...)" (an AND of every enclosing condition) -
/// before that fix, a single boolean flag meant the first "@@endif" at any depth reset the
/// condition regardless of how many "@@if"s were still open;</item>
/// <item>the existing "@@replace(search,replacement)" command, which already applies one or
/// more replacements in order to the text built by "@@start...@@end" - this is what makes
/// chained VB "Replace(Replace(x, a, b), c, d)" expressions convertible without any interpreter
/// change.</item>
/// </list>
/// Scripts are built with <see cref="Script"/> (joining lines with <see cref="Environment.NewLine"/>)
/// rather than multi-line C# string literals, so tests don't depend on the source file's own
/// line-ending / git-checkout settings matching <see cref="Environment.NewLine"/> - which is
/// exactly what <see cref="SimpleScriptInterpreter.Interpret"/> normalizes against internally.
/// </summary>
public class SimpleScriptInterpreterTests
{
    private static string Script(params string[] lines) => string.Join(Environment.NewLine, lines);

    // -----------------------------------------------------------------------
    // IsSimpleScript
    // -----------------------------------------------------------------------

    [Fact]
    public void IsSimpleScript_StartsWithAtAtStart_ReturnsTrue()
    {
        Assert.True(SimpleScriptInterpreter.IsSimpleScript(Script("@@start", "Hello", "@@end")));
    }

    [Fact]
    public void IsSimpleScript_PlainText_ReturnsFalse()
    {
        Assert.False(SimpleScriptInterpreter.IsSimpleScript("[FIELD]"));
    }

    [Fact]
    public void IsSimpleScript_EmptyString_ReturnsFalse()
    {
        Assert.False(SimpleScriptInterpreter.IsSimpleScript(""));
    }

    // -----------------------------------------------------------------------
    // Non-script passthrough
    // -----------------------------------------------------------------------

    [Fact]
    public void Interpret_TextNotStartingWithAtAtStart_ReturnsUnchanged()
    {
        var result = new SimpleScriptInterpreter("Hausanschluss").Interpret();

        Assert.Equal("Hausanschluss", result);
    }

    [Fact]
    public void Interpret_AtAtStartWithoutFollowingNewline_ReturnsUnchanged()
    {
        // "@@start" must be followed by a line break to be treated as a script - a value that
        // merely starts with the literal text "@@start" (e.g. a field whose value happens to be
        // that) is passed through as-is.
        var result = new SimpleScriptInterpreter("@@startsWith").Interpret();

        Assert.Equal("@@startsWith", result);
    }

    // -----------------------------------------------------------------------
    // Plain content lines (no @@if at all)
    // -----------------------------------------------------------------------

    [Fact]
    public void Interpret_PlainLines_JoinsWithEnvironmentNewLine()
    {
        var script = Script("@@start", "Hello", "World", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal($"Hello{Environment.NewLine}World", result);
    }

    [Fact]
    public void Interpret_NoLinesBetweenStartAndEnd_ReturnsEmptyString()
    {
        var script = Script("@@start", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("", result);
    }

    // -----------------------------------------------------------------------
    // Single (non-nested) @@if / @@endif
    // -----------------------------------------------------------------------

    [Fact]
    public void Interpret_SingleIfTrue_IncludesLine()
    {
        var script = Script("@@start", "@@if(yes)", "Shown", "@@endif", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Shown", result);
    }

    [Fact]
    public void Interpret_SingleIfFalse_ExcludesLine()
    {
        var script = Script("@@start", "@@if()", "Hidden", "@@endif", "Always", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Always", result);
    }

    [Fact]
    public void Interpret_ExcludedLeadingLine_LeavesNoBlankSeparatorBeforeNextLine()
    {
        // The join logic only prepends a separator once something has already been appended, so
        // dropping the first line must not leave a stray leading blank line - this is exactly
        // the behaviour AprxLabelExpressionParser's exhaustive verification relies on.
        var script = Script("@@start", "@@if()", "Hidden", "@@endif", "Second", "Third", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal($"Second{Environment.NewLine}Third", result);
    }

    [Fact]
    public void Interpret_UnmatchedEndif_IsIgnoredAndDoesNotThrow()
    {
        var script = Script("@@start", "@@endif", "Content", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Content", result);
    }

    // -----------------------------------------------------------------------
    // Nested @@if / @@endif - the stack-based AND semantics
    // -----------------------------------------------------------------------

    [Fact]
    public void Interpret_NestedIf_BothTrue_IncludesLine()
    {
        var script = Script("@@start", "@@if(a)", "@@if(b)", "Both", "@@endif", "@@endif", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Both", result);
    }

    [Fact]
    public void Interpret_NestedIf_InnerFalse_ExcludesLine()
    {
        var script = Script("@@start", "@@if(a)", "@@if()", "Both", "@@endif", "@@endif", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("", result);
    }

    [Fact]
    public void Interpret_NestedIf_OuterFalseInnerTrue_ExcludesEntireOuterBlock()
    {
        // Regression test for the pre-fix bug: the interpreter used to track "@@if" state with a
        // single boolean instead of a stack, so the *first* "@@endif" reached at any nesting
        // depth reset that flag to true - meaning content between an inner "@@endif" and its
        // enclosing (still-open, and here false) outer "@@if" would incorrectly be shown.
        // With a false outer condition, nothing inside it - including content after the inner
        // "@@endif" but before the outer one - may appear, no matter what the inner condition was.
        var script = Script(
            "@@start",
            "@@if()",       // outer: false
            "@@if(b)",      // inner: true
            "Inner",
            "@@endif",      // closes inner only
            "StillOuter",   // still inside the (false) outer block
            "@@endif",      // closes outer
            "Outside",
            "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Outside", result);
    }

    [Fact]
    public void Interpret_ThreeLevelNestedIf_AllTrue_IncludesLine()
    {
        var script = Script(
            "@@start",
            "@@if(a)", "@@if(b)", "@@if(c)",
            "Deep",
            "@@endif", "@@endif", "@@endif",
            "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Deep", result);
    }

    [Fact]
    public void Interpret_ThreeLevelNestedIf_MiddleFalse_ExcludesLine()
    {
        var script = Script(
            "@@start",
            "@@if(a)", "@@if()", "@@if(c)",
            "Deep",
            "@@endif", "@@endif", "@@endif",
            "After",
            "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("After", result);
    }

    // -----------------------------------------------------------------------
    // @@if(...) condition forms (1/2/3-argument CheckCondition)
    // -----------------------------------------------------------------------

    [Theory]
    // 1-arg: non-empty check ("@@if([Field])" once the field's value has been substituted in).
    [InlineData("yes", true)]
    [InlineData("", false)]
    [InlineData(" ", false)] // whitespace-only counts as "empty" for the 1-arg form specifically
    public void Interpret_SingleArgumentIf_IsNonEmptyCheck(string arg, bool expectedIncluded)
    {
        var script = Script("@@start", $"@@if({arg})", "X", "@@endif", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal(expectedIncluded ? "X" : "", result);
    }

    [Theory]
    // 2-arg: plain equality - this is how AprxLabelExpressionParser implements both
    // "[Field] = 'Value'" (@@if(Value's substituted text,Value)) and, via a trailing empty
    // argument, "[Field] is empty" (@@if(substituted text,)).
    [InlineData("abc,abc", true)]
    [InlineData("abc,def", false)]
    [InlineData(",", true)]     // field substituted to "" and compared against "" -> equal
    [InlineData("abc,", false)] // field substituted to "abc" compared against "" -> not equal
    public void Interpret_TwoArgumentIf_IsEqualityCheck(string args, bool expectedIncluded)
    {
        var script = Script("@@start", $"@@if({args})", "X", "@@endif", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal(expectedIncluded ? "X" : "", result);
    }

    [Theory]
    // 3-arg: explicit "eq"/"not" operator - this is how "[Field] <> 'Value'" against a non-empty
    // literal is expressed (@@if(substituted text,not,Value)).
    [InlineData("abc,eq,abc", true)]
    [InlineData("abc,eq,xyz", false)]
    [InlineData("abc,not,xyz", true)]
    [InlineData("abc,not,abc", false)]
    [InlineData("abc,unknownop,abc", false)] // unrecognized operator -> condition is false
    public void Interpret_ThreeArgumentIf_UsesEqOrNotOperator(string args, bool expectedIncluded)
    {
        var script = Script("@@start", $"@@if({args})", "X", "@@endif", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal(expectedIncluded ? "X" : "", result);
    }

    [Fact]
    public void Interpret_MoreThanThreeArguments_ConditionIsFalse()
    {
        // A field value containing a comma (e.g. "1,2,3") shifts the argument count - documents
        // the existing (pre-existing, not introduced by the converter) limitation that such
        // values can't be safely used inside "@@if(...)".
        var script = Script("@@start", "@@if(a,b,c,d)", "X", "@@endif", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("", result);
    }

    [Theory]
    // Variable-arg "in" operator: "value,in,v1,v2,...,vN" - true if value equals any of v1..vN.
    // Lets AprxLabelExpressionParser collapse several near-duplicate "@@if(...)" branches that
    // only differ by one field's specific Equals value into a single check.
    [InlineData("N,in,N,M,H,F", true)]
    [InlineData("M,in,N,M,H,F", true)]
    [InlineData("F,in,N,M,H,F", true)]
    [InlineData("X,in,N,M,H,F", false)]
    [InlineData(",in,N,M,H,F", false)]
    [InlineData("N,IN,N,M,H,F", true)] // operator keyword is case-insensitive, like eq/not/lt/.../ge
    public void Interpret_InOperator_IsMembershipCheck(string args, bool expectedIncluded)
    {
        var script = Script("@@start", $"@@if({args})", "X", "@@endif", "@@end");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal(expectedIncluded ? "X" : "", result);
    }

    [Fact]
    public void Interpret_InOperator_WithSingleValue_BehavesLikeEquality()
    {
        var scriptMatch = Script("@@start", "@@if(N,in,N)", "X", "@@endif", "@@end");
        var scriptNoMatch = Script("@@start", "@@if(M,in,N)", "X", "@@endif", "@@end");

        Assert.Equal("X", new SimpleScriptInterpreter(scriptMatch).Interpret());
        Assert.Equal("", new SimpleScriptInterpreter(scriptNoMatch).Interpret());
    }

    // -----------------------------------------------------------------------
    // @@replace(search,replacement) - applied in order to the built text
    // -----------------------------------------------------------------------

    [Fact]
    public void Interpret_SingleReplace_AppliesReplacement()
    {
        var script = Script("@@start", "Hausanschluss", "@@end", "@@replace(Hausanschluss,HA)");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("HA", result);
    }

    [Fact]
    public void Interpret_ChainedReplace_AppliesInOrder()
    {
        // Mirrors the real "Beschriftung = Replace(Replace([TYP], a, b), c, d)" chain: each
        // "@@replace" line re-applies to the result of the previous one.
        var script = Script(
            "@@start",
            "Hausanschluss",
            "@@end",
            "@@replace(Hausanschluss,HA)",
            "@@replace(HA,Hausanschluss (HA))");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Hausanschluss (HA)", result);
    }

    [Fact]
    public void Interpret_ReplaceWithEmptyReplacement_RemovesMatch()
    {
        var script = Script("@@start", "Sonstiger Endpunkt", "@@end", "@@replace(Sonstiger Endpunkt,)");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("", result);
    }

    [Fact]
    public void Interpret_ReplaceWithNoMatch_LeavesTextUnchanged()
    {
        var script = Script("@@start", "Schacht", "@@end", "@@replace(Ventil,V)");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Schacht", result);
    }

    [Fact]
    public void Interpret_ReplaceIsCaseSensitive()
    {
        var script = Script("@@start", "Hausanschluss", "@@end", "@@replace(hausanschluss,HA)");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Hausanschluss", result); // different case -> no match, unchanged
    }

    [Fact]
    public void Interpret_ReplaceWithWrongArgumentCount_IsIgnored()
    {
        var script = Script("@@start", "Hausanschluss", "@@end", "@@replace(Hausanschluss)");

        var result = new SimpleScriptInterpreter(script).Interpret();

        Assert.Equal("Hausanschluss", result); // needs exactly 2 arguments - silently skipped
    }

    [Fact]
    public void Interpret_ReplaceCombinedWithConditionalContent_FullPipeline()
    {
        // A full, self-contained script shaped like what AprxLabelExpressionParser generates:
        // a conditionally-included block, followed by post-processing replacements.
        var script = Script(
            "@@start",
            "@@if(a)",
            "Hello World",
            "@@endif",
            "@@end",
            "@@replace(World,Universe)");

        Assert.Equal("Hello Universe", new SimpleScriptInterpreter(script).Interpret());

        var scriptExcluded = script.Replace("@@if(a)", "@@if()");
        Assert.Equal("", new SimpleScriptInterpreter(scriptExcluded).Interpret());
    }

    // -----------------------------------------------------------------------
    // Misc
    // -----------------------------------------------------------------------

    [Fact]
    public void Script_ReturnsConstructorValue()
    {
        var interpreter = new SimpleScriptInterpreter("[FIELD]");

        Assert.Equal("[FIELD]", interpreter.Script);
    }
}
