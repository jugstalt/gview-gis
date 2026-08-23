using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests that constructs outside the deliberately small subset <see cref="AprxLabelExpressionParser"/>
/// supports are rejected (<c>TryConvert</c> returns <see langword="false"/>) rather than producing
/// a wrong or partially-translated result - the caller (<c>AprxMapConverter</c>) falls back to
/// keeping the original expression with a warning in that case, which is always safe.
/// </summary>
public class AprxLabelExpressionParserRejectionTests
{
    private static void AssertRejected(string source)
    {
        var ok = AprxLabelExpressionParser.TryConvert(source, out var result);

        Assert.False(ok, $"Expected the expression to be rejected, but it converted to:\n{result?.Expression}");
        Assert.Null(result);
    }

    [Fact]
    public void FunctionCall_Trim_IsRejected()
    {
        AssertRejected("Trim([FIELD]) & \" x\"");
    }

    [Fact]
    public void UnknownFunctionCall_IsRejected()
    {
        AssertRejected("SomeUnknownFunction([FIELD])");
    }

    [Fact]
    public void Round_WithThreeArguments_IsRejected()
    {
        AssertRejected("round([H], 2, 3)");
    }

    [Fact]
    public void Round_WithUnknownInnerFunction_IsRejected()
    {
        AssertRejected("round(unknown_fn([H]), 2)");
    }

    [Fact]
    public void Round_WithoutDigitCount_IsRejected()
    {
        AssertRejected("round([H])");
    }

    [Fact]
    public void EmptyString_IsRejected()
    {
        AssertRejected("");
    }

    [Fact]
    public void WhitespaceOnly_IsRejected()
    {
        AssertRejected("   ");
    }

    [Fact]
    public void OrOfCrossFieldAndGroup_IsRejected()
    {
        // "A and B or C" parses per standard VB precedence as "(A and B) or C" (and does - see
        // AprxLabelExpressionParserConditionalTests for a case that resolves cleanly) - but *this*
        // one still can't be safely guarded: nothing here lets us tell whether "(A and B)" is
        // already true while building "C"'s guard (different fields, no auto-contradiction), and
        // without that, both could fire at once and duplicate the output. Correctly rejected
        // rather than risking that.
        AssertRejected("""
            Function F([A],[B],[C])
            if [A] <> "" and [B] <> "" or [C] <> "" then
             F = [A] & [B] & [C]
            end if
            End Function
            """);
    }

    [Fact]
    public void ReplaceChain_TargetVariableMismatch_IsRejected()
    {
        // The chain must keep reassigning the *same* local variable throughout.
        AssertRejected("""
            Function F([A])
            X = Replace([A],"a","b")
            F = Y
            End Function
            """);
    }

    [Fact]
    public void ReplaceChain_CommaInsideReplacementValue_IsRejected()
    {
        // gView's "@@replace(search,replacement)" splits its arguments on a bare "," with no
        // escaping - a "," inside either value would be silently mis-parsed at runtime, so this
        // is rejected rather than risking a subtly wrong generated script.
        AssertRejected("""
            Function F([A])
            X = Replace([A],"a,b","c")
            F = X
            End Function
            """);
    }

    [Fact]
    public void ReplaceChain_EmptySearchValue_IsRejected()
    {
        AssertRejected("""
            Function F([A])
            X = Replace([A],"","c")
            F = X
            End Function
            """);
    }

    [Fact]
    public void NestedIf_IsRejected()
    {
        AssertRejected("""
            Function F([A],[B])
            if [A] <> "" then
             if [B] <> "" then
              F = [A] & [B]
             end if
            end if
            End Function
            """);
    }

    // Numeric range comparisons (<, <=, >, >=) and equality/inequality against an unquoted
    // number (=, <>) themselves ARE supported - see AprxLabelExpressionParserConditionalTests.
    // This is a remaining unsupported shape around them.

    [Fact]
    public void NumericComparisonAgainstFunctionCall_IsRejected()
    {
        AssertRejected("""
            Function F([A],[B])
            if [A] > Len([B]) then
             F = [A]
            end if
            End Function
            """);
    }

    [Fact]
    public void EqualityLiteral_ContainingComma_IsRejected()
    {
        // gView's "@@if([Field],Value)" splits its arguments on a bare "," with no escaping - a
        // "," inside the literal would be silently mis-parsed at runtime.
        AssertRejected("""
            Function F([A])
            if [A] = "x,y" then
             F = [A]
            end if
            End Function
            """);
    }

    [Fact]
    public void MalformedFieldReference_UnclosedBracket_IsRejected()
    {
        AssertRejected("\"prefix\" & [FIELD");
    }

    [Fact]
    public void UnterminatedDoubleQuotedString_IsRejected()
    {
        AssertRejected("\"prefix & [FIELD]");
    }

    [Fact]
    public void UnterminatedSingleQuotedString_IsRejected()
    {
        // A stray, unmatched "'" (e.g. a typo right after a properly closed "..." literal) opens
        // a single-quoted string that's never closed - must be rejected, not silently swallow the
        // rest of the expression (including any following [Field] references) into a literal.
        AssertRejected("\"proj.\"' & [BESCHR]");
    }

    [Fact]
    public void DollarFeatureNotFollowedByFieldName_IsRejected()
    {
        AssertRejected("$feature.");
    }

}
