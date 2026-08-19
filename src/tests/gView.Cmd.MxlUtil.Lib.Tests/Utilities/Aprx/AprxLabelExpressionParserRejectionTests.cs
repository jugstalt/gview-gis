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
    public void OrConditionWithoutParentheses_MixedWithAnd_IsRejectedAsAmbiguous()
    {
        // "or" mixed with "and" but not parenthesized to disambiguate precedence - rejected
        // rather than guessed at.
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

    [Fact]
    public void NumericComparison_IsRejected()
    {
        AssertRejected("""
            Function F([A])
            if [A] > 5 then
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
}
