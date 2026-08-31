using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

/// <summary>
/// Attempts to translate an ArcGIS Pro (VBScript-like) label expression into a gView label
/// expression: plain text with <c>[FieldName]</c> placeholders, optionally wrapped in gView's
/// <c>@@start</c> / <c>@@if(...)</c> / <c>@@endif</c> / <c>@@end</c> mini-script
/// (see <see cref="gView.Framework.Common.SimpleScriptInterpreter"/>) when the ArcGIS Pro
/// expression conditionally includes a text line only if a field is not empty.
/// </summary>
/// <remarks>
/// This only handles a deliberately small subset of VBScript (plus a couple of ArcGIS Pro's own
/// non-standard conveniences, noted below):
/// <list type="bullet">
/// <item>a plain string literal, e.g. <c>"Leerrohr vorh."</c> or <c>'Leerrohr vorh.'</c> - ArcGIS
/// Pro's label-expression parser accepts single quotes as an alternate string delimiter too
/// (unlike strict VBScript, where <c>'</c> only starts a comment). Any ArcGIS Pro rich-text
/// formatting tags inside a literal (<c>&lt;CLR .../&gt;</c>, <c>&lt;BOL&gt;</c>, <c>&lt;FNT
/// .../&gt;</c>, ...) are stripped, since gView's label text symbols can't render per-run
/// formatting - see <see cref="RichTextTag"/>.</item>
/// <item>a concatenation (via <c>&amp;</c>, <c>+</c>, or the non-standard-but-tolerated <c>%</c> -
/// a likely "&amp;" typo, common enough in real source data to accept rather than reject) of
/// string literals, <c>[Field]</c> references, Arcade's <c>$feature.Field</c> syntax (treated the
/// same as <c>[Field]</c>), a numeric/string cast wrapping a field (VB's <c>float</c>/<c>cdbl</c>/
/// <c>cdec</c>/<c>csng</c>/<c>val</c>/<c>cint</c>/<c>clng</c>, Python's <c>int</c>/<c>str</c>/
/// <c>repr</c> - all no-ops for our purposes), <c>round(...)</c> calls, and <c>vbNewLine</c>/
/// <c>vbCrLf</c>/<c>vbTab</c> (also tolerating the common misspelling <c>vbNewNLine</c>), e.g.
/// <c>"lt. SAP: " &amp; [LABEL_SAP]</c> or <c>[SCHIEBERTYP] +[STATNR]</c></item>
/// <item>Arcade's geometry accessors <c>Length($feature)</c>/<c>$feature.Length</c> (line length,
/// or polygon perimeter) and <c>Area($feature)</c>/<c>$feature.Area</c> (polygon area), bare or
/// wrapped in <c>round(...)</c> - only a bare <c>$feature</c> argument is understood, nothing else
/// is guessed at (e.g. <c>Length($feature.SubField)</c> is rejected). These map onto the reserved
/// pseudo-field placeholders <c>[$feature.length]</c>/<c>[$feature.area]</c> (see
/// <see cref="GeometryLengthFieldName"/>/<see cref="GeometryAreaFieldName"/>), resolved at
/// label-render time from the feature's actual geometry - see
/// <see cref="gView.Framework.Cartography.Rendering.SimpleLabelRenderer"/> and
/// <see cref="gView.Framework.Core.Geometry.Extensions.GeometryExtensions.GetLength"/>/
/// <see cref="gView.Framework.Core.Geometry.Extensions.GeometryExtensions.GetArea"/> - rather than
/// from an attribute field, unlike every other placeholder this parser produces. The computed
/// value is planar (Euclidean, in the feature's own native/unprojected coordinate units), not
/// geodesic like ArcGIS Pro's own <c>Length</c>/<c>Area</c> typically are - numbers can differ from
/// the original ArcGIS Pro label, especially under a geographic coordinate system or for very
/// long/large features. "Length"/"Area" are always assumed to mean this geometry accessor, never a
/// same-named real attribute field (which would be indistinguishable from source text alone) -
/// <see cref="gView.Cmd.MxlUtil.Lib.Utilities.ConvertAprx"/> surfaces an informational note
/// whenever this assumption is made, so a conversion can be spot-checked if the source schema
/// genuinely has such a field.</item>
/// <item>a <c>Function ... End Function</c> wrapper around a single such assignment</item>
/// <item>a <c>Function ... End Function</c> wrapper with an <c>If/ElseIf/[Else]/End If</c> chain,
/// where every branch condition is built from <c>[Field] &lt;&gt; ""</c> / <c>[Field] = ""</c> /
/// <c>[Field] = "Value"</c> / <c>[Field] &lt;&gt; "Value"</c> / <c>[Field] = Number</c> /
/// <c>[Field] &lt;&gt; Number</c> / <c>[Field] (&lt;|&lt;=|&gt;|&gt;=) Number</c> / VB's
/// <c>IsNull([Field])</c>/<c>Not IsNull([Field])</c> / Python's <c>[Field] is None</c>/
/// <c>[Field] is not None</c> checks (also accepting Python's <c>==</c>/<c>!=</c> as synonyms for
/// <c>=</c>/<c>&lt;&gt;</c>), combined with <c>and</c>/<c>or</c> and arbitrarily nested/redundant
/// parentheses, following standard VB precedence (<c>and</c> binds tighter than <c>or</c>) - see
/// <see cref="ParseConditionExpanded"/>. Each branch's own text is reproduced verbatim behind a
/// guard that reconstructs VB's first-match semantics - see
/// <see cref="TryBuildConditionalScript"/>. Not every such condition can actually be built into a
/// guard (e.g. an <c>or</c> of two branches that don't share a contradicting field would risk
/// duplicated output) - those are rejected rather than risking it.</item>
/// <item>a <c>Function ... End Function</c> wrapper that repeatedly *appends* to a base value
/// through a series of independent, standalone <c>If ... Then / NAME = NAME &amp; ... / End If</c>
/// blocks (not an If/ElseIf/Else chain - each is a separate "if", so several can fire together)
/// - see <see cref="TryParseConditionalAppendChain"/> - or that repeatedly narrows a local
/// variable through chained VB <c>Replace(...)</c> calls, mapped onto gView's existing
/// <c>@@replace(search,replacement)</c> mini-script command - see
/// <see cref="TryParseReplaceChain"/>.</item>
/// <item>ArcGIS Pro's Python-flavored label expression type, <c>def name(...): ...</c>, with the
/// same conditions as above (VB's "and"/"or" are spelled identically) but Python's own statement
/// syntax: indentation-delimited <c>if COND: / elif COND: / [else:]</c> blocks whose bodies are
/// each exactly one, more-indented <c>return EXPR</c> line - see
/// <see cref="TryConvertPythonFunction"/>. A <c>\n</c>/<c>\t</c> escape sequence inside a string
/// literal is decoded (real newline/tab), unlike VBScript, where a backslash is always just an
/// ordinary character.</item>
/// </list>
/// Anything else (function calls other than the ones named above, arithmetic, nested ifs,
/// multi-statement Python if/elif/else bodies, malformed/unterminated string literals, likely
/// source typos we can't confidently guess-fix, ...) is rejected so the caller can fall back to
/// keeping the original expression with a warning. Every conditional reduction is exhaustively
/// verified against every possible combination of field values worth distinguishing before being
/// accepted, so a rejected (unsupported) shape never produces wrong output - it only ever falls
/// back to the warning.
/// <para>
/// For an If/ElseIf/Python-elif chain, once the per-branch guards are verified correct, a final
/// readability pass (<see cref="CoalesceForRendering"/>) merges branches that agree on everything
/// - other conditions, exclude-guards, and output text - except one field's specific Equals value,
/// using gView's variable-arg <c>@@if([Field],in,V1,V2,...)</c> form instead of one near-duplicate
/// "@@if" block per value. This is purely cosmetic: it can turn e.g. a 16-branch script (two
/// independently-enumerated fields) into 2, but never changes behaviour - the merged result is
/// re-verified the same exhaustive way before use, and silently falls back to the (already
/// verified) unmerged rendering if that ever fails.
/// </para>
/// </remarks>
internal static class AprxLabelExpressionParser
{
    public sealed record ConversionResult(string Expression, bool IsConditional);

    /// <summary>
    /// Tries to convert <paramref name="source"/> (an ArcGIS Pro label expression) into a gView
    /// label expression. Returns <see langword="false"/> if the expression uses constructs that
    /// cannot be safely reduced to gView's placeholder-text (optionally conditional) format.
    /// </summary>
    public static bool TryConvert(string source, out ConversionResult? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        var text = source.Trim();

        // "Function Name(...) ... End Function" wrapper
        var funcMatch = Regex.Match(
            text,
            @"^Function\s+(?<name>\w+)\s*\([^)]*\)\s*(?<body>.*?)End\s+Function\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (funcMatch.Success)
        {
            return TryConvertFunction(funcMatch.Groups["name"].Value, funcMatch.Groups["body"].Value, out result);
        }

        // "def name(...):" wrapper - ArcGIS Pro's Python-flavored label expression type
        var pyMatch = Regex.Match(
            text,
            @"^def\s+\w+\s*\([^)]*\)\s*:(?<body>.*)$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (pyMatch.Success)
        {
            return TryConvertPythonFunction(pyMatch.Groups["body"].Value, out result);
        }

        // Plain expression: literal text and/or "&" concatenation, no function wrapper
        if (TryTokenizeConcatExpression(text, out var terms) && terms.Count > 0)
        {
            result = new ConversionResult(RenderTerms(terms), IsConditional: false);
            return true;
        }

        return false;
    }

    // -----------------------------------------------------------------------
    // Function / If-ElseIf-Else handling
    // -----------------------------------------------------------------------

    private sealed record Branch(List<Predicate> Conditions, List<Term> Terms);

    private static bool TryConvertFunction(string funcName, string body, out ConversionResult? result)
    {
        result = null;

        if (TryParseReplaceChain(funcName, body, out result))
        {
            return true;
        }

        if (TryParseConditionalAppendChain(funcName, body, out result))
        {
            return true;
        }

        var branches = ParseIfElseBranches(funcName, body);
        if (branches == null || branches.Count == 0)
        {
            return false;
        }

        if (branches.Count == 1 && branches[0].Conditions.Count == 0)
        {
            // No conditional logic at all (e.g. "Function X(...)\nX = ... \nEnd Function").
            // A single "If ... Then ... End If" with no "Else" still needs the conditional
            // path below - without an Else, VB returns an empty string when the condition is
            // false, which TryBuildConditionalScript's guard reconstructs correctly.
            result = new ConversionResult(RenderTerms(branches[0].Terms), IsConditional: false);
            return true;
        }

        return TryBuildConditionalScript(branches, out result);
    }

    // -----------------------------------------------------------------------
    // Python-flavored "def name(...): ..." handling (another label-expression type ArcGIS Pro
    // supports, alongside VBScript and Arcade). Conditions and branch-guard reconstruction reuse
    // the exact same Predicate/Branch/TryBuildConditionalScript machinery as the VB Function path
    // - "and"/"or" are spelled the same in both languages, and Python's "=="/"!=" are already
    // accepted as synonyms for VB's "="/"<>" (see ParseConditionAtom). Only the outer statement
    // syntax (indentation-delimited "if/elif/else:" blocks with "return EXPR" bodies, instead of
    // "If/ElseIf/Else/End If" with "NAME = EXPR") differs.
    // -----------------------------------------------------------------------

    private static bool TryConvertPythonFunction(string body, out ConversionResult? result)
    {
        result = null;

        var lines = SplitPythonLines(body);
        if (lines.Count == 0)
        {
            return false;
        }

        var branches = ParsePythonBlock(lines, 0, lines.Count, out var consumed);
        if (branches == null || consumed != lines.Count)
        {
            return false; // unsupported shape, or trailing content after the if/elif/else chain
        }

        if (branches.Count == 1 && branches[0].Conditions.Count == 0)
        {
            result = new ConversionResult(RenderTerms(branches[0].Terms), IsConditional: false);
            return true;
        }

        return TryBuildConditionalScript(branches, out result);
    }

    /// <summary>Splits a Python function body into (indentColumns, trimmedText) for non-blank, non-comment lines.</summary>
    private static List<(int Indent, string Text)> SplitPythonLines(string body)
    {
        var result = new List<(int Indent, string Text)>();
        foreach (var raw in body.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
        {
            var trimmedEnd = raw.TrimEnd();
            var text = trimmedEnd.TrimStart(' ', '\t');
            if (text.Length == 0 || text.StartsWith("#"))
            {
                continue; // blank or comment line
            }
            result.Add((trimmedEnd.Length - text.Length, text));
        }
        return result;
    }

    /// <summary>
    /// Parses one Python statement sequence starting at <c>lines[start]</c>: either a single
    /// <c>return EXPR</c> (unconditional), or an <c>if COND: / elif COND: / [else:]</c> chain -
    /// all at <c>lines[start].Indent</c>'s indentation level - whose bodies are each exactly one,
    /// more-indented <c>return EXPR</c> line. This is a deliberately small subset: multi-statement
    /// bodies, nested ifs, and anything else are rejected rather than guessed at. Sets
    /// <paramref name="consumed"/> to how many lines (from <paramref name="start"/>) were used,
    /// and returns <see langword="null"/> if the shape doesn't match.
    /// </summary>
    private static List<Branch>? ParsePythonBlock(List<(int Indent, string Text)> lines, int start, int end, out int consumed)
    {
        consumed = start;
        if (start >= end)
        {
            return null;
        }

        var bareReturn = Regex.Match(lines[start].Text, @"^return(?:\s+(?<expr>.+))?$", RegexOptions.IgnoreCase);
        if (bareReturn.Success)
        {
            var exprText = bareReturn.Groups["expr"].Success ? bareReturn.Groups["expr"].Value.Trim() : "";
            if (!TryTokenizeConcatExpression(exprText, out var terms, pythonEscapes: true))
            {
                return null;
            }
            consumed = start + 1;
            return [new Branch([], terms)];
        }

        var blockIndent = lines[start].Indent;
        var ifMatch = Regex.Match(lines[start].Text, @"^if\s+(?<cond>.+):$", RegexOptions.IgnoreCase);
        if (!ifMatch.Success)
        {
            return null; // unsupported statement shape
        }

        var branches = new List<Branch>();
        int idx = start;
        var currentMatch = ifMatch;
        var isElse = false;

        while (true)
        {
            List<List<Predicate>>? conditionCombinations;
            if (isElse)
            {
                conditionCombinations = [[]];
            }
            else
            {
                conditionCombinations = ParseConditionExpanded(currentMatch.Groups["cond"].Value);
                if (conditionCombinations == null)
                {
                    return null;
                }
            }

            idx++;
            if (idx >= end || lines[idx].Indent <= blockIndent)
            {
                return null; // missing indented body
            }

            var bodyReturn = Regex.Match(lines[idx].Text, @"^return(?:\s+(?<expr>.+))?$", RegexOptions.IgnoreCase);
            if (!bodyReturn.Success)
            {
                return null; // only a single "return EXPR" statement per body is supported
            }

            var bodyExprText = bodyReturn.Groups["expr"].Success ? bodyReturn.Groups["expr"].Value.Trim() : "";
            if (!TryTokenizeConcatExpression(bodyExprText, out var branchTerms, pythonEscapes: true))
            {
                return null;
            }
            idx++;

            if (idx < end && lines[idx].Indent > blockIndent)
            {
                return null; // more than one statement in this if/elif/else body - not supported
            }

            foreach (var conditions in conditionCombinations)
            {
                branches.Add(new Branch(conditions, branchTerms));
            }

            if (isElse || idx >= end || lines[idx].Indent != blockIndent)
            {
                break; // no more elif/else at this level - chain ends (no implicit else)
            }

            var elifMatch = Regex.Match(lines[idx].Text, @"^elif\s+(?<cond>.+):$", RegexOptions.IgnoreCase);
            if (elifMatch.Success)
            {
                currentMatch = elifMatch;
                continue;
            }

            if (Regex.IsMatch(lines[idx].Text, @"^else\s*:$", RegexOptions.IgnoreCase))
            {
                isElse = true;
                continue;
            }

            break; // next line at this indent isn't part of the chain
        }

        consumed = idx;
        return branches;
    }

    /// <summary>Splits a VB function body into trimmed, non-blank, non-comment lines.</summary>
    private static List<string> SplitBodyLines(string body) =>
        body.Replace("\r\n", "\n").Replace("\r", "\n")
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith("'"))
            .ToList();

    /// <summary>
    /// Tries to parse a VB <c>Function</c> body that repeatedly narrows a local variable through
    /// chained <c>Replace(source, "search", "replacement")</c> calls, e.g.:
    /// <code>
    /// Beschriftung = Replace([TYP], "Hausanschluss", "HA")
    /// Beschriftung = Replace(Beschriftung, "Reserve", "Res.")
    /// FindLabel = Beschriftung
    /// </code>
    /// This maps directly onto gView's existing <c>@@replace(search,replacement)</c> mini-script
    /// command (see <see cref="gView.Framework.Common.SimpleScriptInterpreter"/>), which already
    /// applies one or more replacements, in order, to the text built by the
    /// "@@start...@@end" block - so no interpreter changes are needed for this shape.
    /// </summary>
    private static bool TryParseReplaceChain(string funcName, string body, out ConversionResult? result)
    {
        result = null;

        var lines = SplitBodyLines(body);
        if (lines.Count < 2)
        {
            return false; // need at least one Replace() line plus the final "funcName = var" line
        }

        var finalMatch = Regex.Match(lines[^1], $@"^{Regex.Escape(funcName)}\s*=\s*(?<var>\w+)\s*$", RegexOptions.IgnoreCase);
        if (!finalMatch.Success)
        {
            return false;
        }

        var localVar = finalMatch.Groups["var"].Value;
        var replaceLineRegex = new Regex(
            "^" + Regex.Escape(localVar) +
            @"\s*=\s*Replace\s*\(\s*(?<source>.+?)\s*,\s*""(?<search>(?:[^""]|"""")*)""\s*,\s*""(?<replace>(?:[^""]|"""")*)""\s*\)\s*$",
            RegexOptions.IgnoreCase);

        List<Term>? baseTerms = null;
        var replacements = new List<(string Search, string Replace)>();

        for (int i = 0; i < lines.Count - 1; i++)
        {
            var m = replaceLineRegex.Match(lines[i]);
            if (!m.Success)
            {
                return false;
            }

            var source = m.Groups["source"].Value.Trim();
            var search = m.Groups["search"].Value.Replace("\"\"", "\"");
            var replace = m.Groups["replace"].Value.Replace("\"\"", "\"");

            // An empty search is ill-defined here, and gView's "@@replace(...)" splits its
            // arguments on a bare "," with no escaping, so a "," inside either value would be
            // silently mis-parsed - bail rather than risk generating a subtly wrong script.
            if (search.Length == 0 || search.Contains(',') || replace.Contains(','))
            {
                return false;
            }

            if (i == 0)
            {
                // The first call's source is the seed value (usually just "[Field]") - anything
                // our normal literal/"&"/[Field] tokenizer accepts.
                if (!TryTokenizeConcatExpression(source, out baseTerms))
                {
                    return false;
                }
            }
            else if (!string.Equals(source, localVar, StringComparison.OrdinalIgnoreCase))
            {
                return false; // every later call must keep chaining off the same local variable
            }

            replacements.Add((search, replace));
        }

        if (baseTerms == null || replacements.Count == 0)
        {
            return false;
        }

        var sb = new StringBuilder();
        sb.Append("@@start").Append(Environment.NewLine);
        foreach (var line in SplitIntoLines(baseTerms))
        {
            sb.Append(RenderTerms(line)).Append(Environment.NewLine);
        }
        sb.Append("@@end");

        foreach (var (search, replace) in replacements)
        {
            sb.Append(Environment.NewLine).Append($"@@replace({search},{replace})");
        }

        result = new ConversionResult(sb.ToString(), IsConditional: true);
        return true;
    }

    /// <summary>
    /// Tries to parse a VB <c>Function</c> body that starts with a base assignment and then
    /// conditionally *appends* to it through a series of independent, standalone
    /// <c>If ... Then / NAME = NAME &amp; ... / End If</c> blocks (not an If/ElseIf/Else chain -
    /// each is a separate "if", so zero, one, or several can fire together for the same feature):
    /// <code>
    /// FindLabel = [KENNUNG] &amp; " " &amp; [NAME]
    /// if [CDMA] = "Ja" then
    ///     FindLabel = FindLabel &amp; vbnewline &amp; "    CDMA"
    /// end if
    /// if [DMR] = "Ja" then
    ///     FindLabel = FindLabel &amp; vbnewline &amp; "    DMR"
    /// end if
    /// </code>
    /// Each append's own condition is turned into a nested <c>@@if</c> guard around its own text,
    /// exactly like a branch in <see cref="TryBuildConditionalScript"/> - but since these appends
    /// are independent (not mutually exclusive alternatives), no exclude-guards between them are
    /// needed or wanted. A leading <c>vbNewLine</c> in an append's own text is dropped (see
    /// <see cref="StripLeadingNewline"/>) since gView's interpreter already inserts that separator
    /// automatically between any two lines it shows.
    /// </summary>
    private static bool TryParseConditionalAppendChain(string funcName, string body, out ConversionResult? result)
    {
        result = null;

        var lines = SplitBodyLines(body);
        if (lines.Count < 4 || (lines.Count - 1) % 3 != 0)
        {
            return false; // base line + N*(if/assign/end if) groups of exactly 3 lines each
        }

        var baseMatch = Regex.Match(lines[0], $@"^{Regex.Escape(funcName)}\s*=\s*(?<expr>.+)$", RegexOptions.IgnoreCase);
        if (!baseMatch.Success || !TryTokenizeConcatExpression(baseMatch.Groups["expr"].Value, out var baseTerms))
        {
            return false;
        }

        var assignRegex = new Regex(
            $@"^{Regex.Escape(funcName)}\s*=\s*{Regex.Escape(funcName)}\s*(?:&|\+|%)\s*(?<expr>.+)$",
            RegexOptions.IgnoreCase);

        var appends = new List<(List<Predicate> Conditions, List<Term> Terms)>();

        for (int i = 1; i < lines.Count; i += 3)
        {
            var ifMatch = Regex.Match(lines[i], @"^if\s+(?<cond>.+?)\s+then$", RegexOptions.IgnoreCase);
            if (!ifMatch.Success)
            {
                return false;
            }

            var conditionCombinations = ParseConditionExpanded(ifMatch.Groups["cond"].Value);
            if (conditionCombinations == null || conditionCombinations.Count != 1)
            {
                return false; // keep this shape simple: exactly one AND-only condition per append
            }

            var assignMatch = assignRegex.Match(lines[i + 1]);
            if (!assignMatch.Success || !TryTokenizeConcatExpression(assignMatch.Groups["expr"].Value, out var appendTerms))
            {
                return false;
            }

            if (!Regex.IsMatch(lines[i + 2], @"^end\s*if$", RegexOptions.IgnoreCase))
            {
                return false;
            }

            appends.Add((conditionCombinations[0], appendTerms));
        }

        var allPredicates = appends.SelectMany(a => a.Conditions).ToList();
        if (!TryBuildFieldDomains(allPredicates, out var fields, out var fieldDomains))
        {
            return false;
        }

        bool VerifyCombination(Dictionary<string, string> values) => VerifyOneAppendCombination(baseTerms, appends, values);
        if (!ForAllCombinations(fields, fieldDomains, VerifyCombination))
        {
            return false;
        }

        result = new ConversionResult(RenderAppendChainScript(baseTerms, appends), IsConditional: true);
        return true;
    }

    /// <summary>
    /// Checks, for one combination of field values, that independently guarding each append
    /// (see <see cref="TryParseConditionalAppendChain"/>) reproduces exactly the same text as the
    /// original VB base-plus-appends would.
    /// </summary>
    private static bool VerifyOneAppendCombination(
        List<Term> baseTerms,
        List<(List<Predicate> Conditions, List<Term> Terms)> appends,
        Dictionary<string, string> values)
    {
        string ValueOf(string field) => values.TryGetValue(field, out var v) ? v : "";
        bool AllTrue(IEnumerable<Predicate> preds) => preds.All(p => p.Evaluate(ValueOf(p.Field)));

        // What the original VB would produce: the base text, then each append's text concatenated
        // on on top (in order) exactly when its own condition holds - a flat concatenation, since
        // vbNewLine is just a literal character to VB; there's no "line" concept on this side.
        var allTerms = new List<Term>(baseTerms);
        foreach (var (conditions, terms) in appends)
        {
            if (AllTrue(conditions))
            {
                allTerms.AddRange(terms);
            }
        }
        var vbText = RenderTermsForComparison(allTerms);

        // What gView's SimpleScriptInterpreter would actually produce: the base line(s) always
        // shown, then each append's own line(s) shown only while its own guard holds - joined the
        // same way the real interpreter joins included lines (a separator only once something's
        // already been appended), with each append's leading NewlineTerm (if any) stripped first,
        // since that separator role is already played by the interpreter's own join logic.
        var sb = new StringBuilder();
        void AppendLines(List<Term> terms)
        {
            foreach (var line in SplitIntoLines(terms))
            {
                if (sb.Length > 0)
                {
                    sb.Append('\n');
                }
                sb.Append(RenderTermsForComparison(line));
            }
        }

        AppendLines(baseTerms);
        foreach (var (conditions, terms) in appends)
        {
            if (AllTrue(conditions))
            {
                AppendLines(StripLeadingNewline(terms));
            }
        }

        return string.Equals(vbText, sb.ToString(), StringComparison.Ordinal);
    }

    /// <summary>Builds the gView "@@start/@@if/@@endif/@@end" mini-script for an append chain.</summary>
    private static string RenderAppendChainScript(List<Term> baseTerms, List<(List<Predicate> Conditions, List<Term> Terms)> appends)
    {
        var sb = new StringBuilder();
        sb.Append("@@start").Append(Environment.NewLine);

        foreach (var line in SplitIntoLines(baseTerms))
        {
            sb.Append(RenderTerms(line)).Append(Environment.NewLine);
        }

        foreach (var (conditions, terms) in appends)
        {
            int depth = 0;
            foreach (var predicate in conditions)
            {
                sb.Append($"@@if({predicate.RenderIfArgs()})").Append(Environment.NewLine);
                depth++;
            }

            foreach (var line in SplitIntoLines(StripLeadingNewline(terms)))
            {
                sb.Append(RenderTerms(line)).Append(Environment.NewLine);
            }

            for (int d = 0; d < depth; d++)
            {
                sb.Append("@@endif").Append(Environment.NewLine);
            }
        }

        sb.Append("@@end");
        return sb.ToString();
    }

    /// <summary>
    /// Drops a single leading <see cref="NewlineTerm"/>, if present - used for an append block's
    /// own text, whose leading "vbNewLine" only exists in the original VB to separate it from
    /// whatever came before; gView's interpreter already inserts that same separator automatically
    /// between any two lines it shows, so keeping it too would double it up.
    /// </summary>
    private static List<Term> StripLeadingNewline(List<Term> terms) =>
        terms.Count > 0 && terms[0] is NewlineTerm ? terms.Skip(1).ToList() : terms;

    /// <summary>
    /// Parses the body of a VB <c>Function</c> into either a single unconditional branch
    /// (plain assignment) or a list of branches produced by an <c>If/ElseIf/[Else]/End If</c> chain.
    /// Returns <see langword="null"/> if the body uses anything outside that shape.
    /// </summary>
    private static List<Branch>? ParseIfElseBranches(string funcName, string body)
    {
        var rawLines = SplitBodyLines(body);

        if (rawLines.Count == 0)
        {
            return null;
        }

        bool hasIf = rawLines.Any(l => Regex.IsMatch(l, @"^if\b", RegexOptions.IgnoreCase));

        if (!hasIf)
        {
            if (rawLines.Count != 1 || !TryParseAssignment(funcName, rawLines[0], out var terms))
            {
                return null;
            }

            return [new Branch([], terms)];
        }

        var branches = new List<Branch>();
        int idx = 0;

        var ifMatch = Regex.Match(rawLines[idx], @"^if\s+(?<cond>.+?)\s+then\s*(?<rest>.*)$", RegexOptions.IgnoreCase);
        if (!ifMatch.Success)
        {
            return null;
        }

        while (true)
        {
            var conditionCombinations = ParseConditionExpanded(ifMatch.Groups["cond"].Value);
            if (conditionCombinations == null)
            {
                return null;
            }

            idx++;

            List<Term>? branchTerms;
            var inlineRest = ifMatch.Groups["rest"].Value.Trim();
            if (inlineRest.Length > 0)
            {
                if (!TryParseAssignment(funcName, inlineRest, out branchTerms))
                {
                    return null;
                }
            }
            else
            {
                if (idx >= rawLines.Count || !TryParseAssignment(funcName, rawLines[idx], out branchTerms))
                {
                    return null;
                }
                idx++;
            }

            // An "and (... or ...)" condition expands into several AND-only sibling branches -
            // all sharing this same VB clause's text, inserted here in place of the one clause.
            foreach (var conditions in conditionCombinations)
            {
                branches.Add(new Branch(conditions, branchTerms));
            }

            if (idx >= rawLines.Count)
            {
                return null; // missing "End If"
            }

            var next = rawLines[idx];

            var elseIfMatch = Regex.Match(next, @"^else\s*if\s+(?<cond>.+?)\s+then\s*(?<rest>.*)$", RegexOptions.IgnoreCase);
            if (elseIfMatch.Success)
            {
                ifMatch = elseIfMatch;
                continue;
            }

            if (Regex.IsMatch(next, @"^else\s*$", RegexOptions.IgnoreCase))
            {
                idx++;
                if (idx >= rawLines.Count || !TryParseAssignment(funcName, rawLines[idx], out var elseTerms))
                {
                    return null;
                }
                idx++;

                branches.Add(new Branch([], elseTerms));

                if (idx >= rawLines.Count || !Regex.IsMatch(rawLines[idx], @"^end\s*if$", RegexOptions.IgnoreCase))
                {
                    return null;
                }
                idx++;
                break;
            }

            if (Regex.IsMatch(next, @"^end\s*if$", RegexOptions.IgnoreCase))
            {
                idx++;
                break;
            }

            return null; // unexpected line (nested if, comment we didn't strip, ...)
        }

        return idx == rawLines.Count ? branches : null; // trailing garbage after "End If"
    }

    private static bool TryParseAssignment(string funcName, string line, out List<Term> terms)
    {
        terms = [];

        var m = Regex.Match(line, $@"^{Regex.Escape(funcName)}\s*=\s*(?<expr>.+)$", RegexOptions.IgnoreCase);
        if (!m.Success)
        {
            return false;
        }

        return TryTokenizeConcatExpression(m.Groups["expr"].Value, out terms);
    }

    /// <summary>What a single condition atom checks about one field's value.</summary>
    private enum PredicateKind { NonEmpty, Empty, Equals, NotEquals, LessThan, LessOrEqual, GreaterThan, GreaterOrEqual, In }

    /// <summary>
    /// One VB condition atom on a single field, e.g. <c>[FIELD] &lt;&gt; ""</c>,
    /// <c>[FIELD] = "Schieber"</c>, or a numeric comparison like <c>[FIELD] &lt;= 100</c>.
    /// Mirrors exactly what gView's <see cref="gView.Framework.Common.SimpleScriptInterpreter"/>
    /// checks for the matching <c>@@if(...)</c> form, so <see cref="Evaluate"/> can be used to
    /// predict its real behaviour.
    /// </summary>
    private sealed record Predicate(string Field, PredicateKind Kind, string? Value = null)
    {
        /// <summary>Evaluates this predicate exactly like the corresponding <c>@@if(...)</c> would.</summary>
        public bool Evaluate(string fieldValue) => Kind switch
        {
            PredicateKind.NonEmpty => fieldValue.Length > 0,
            PredicateKind.Empty => fieldValue.Length == 0,
            PredicateKind.Equals => string.Equals(fieldValue, Value, StringComparison.Ordinal),
            PredicateKind.NotEquals => !string.Equals(fieldValue, Value, StringComparison.Ordinal),
            PredicateKind.LessThan => TryCompareNumeric(fieldValue, Value, out var c1) && c1 < 0,
            PredicateKind.LessOrEqual => TryCompareNumeric(fieldValue, Value, out var c2) && c2 <= 0,
            PredicateKind.GreaterThan => TryCompareNumeric(fieldValue, Value, out var c3) && c3 > 0,
            PredicateKind.GreaterOrEqual => TryCompareNumeric(fieldValue, Value, out var c4) && c4 >= 0,
            // Only ever produced by CoalesceForRendering, merging several Equals branches on the
            // same field into one "value is one of ..." check - see its remarks.
            PredicateKind.In => DecodeInValues(Value!).Any(v => string.Equals(fieldValue, v, StringComparison.Ordinal)),
            _ => false
        };

        public Predicate Negate() => Kind switch
        {
            PredicateKind.NonEmpty => this with { Kind = PredicateKind.Empty },
            PredicateKind.Empty => this with { Kind = PredicateKind.NonEmpty },
            PredicateKind.Equals => this with { Kind = PredicateKind.NotEquals },
            PredicateKind.NotEquals => this with { Kind = PredicateKind.Equals },
            // Mathematically exact only when the field parses as a number; when it doesn't, both
            // this predicate AND its negation evaluate false (see Evaluate above), which just
            // means that field's own branch never fires either way - harmless, see
            // TryBuildConditionalScript's remarks for why that's still exhaustively verified safe.
            PredicateKind.LessThan => this with { Kind = PredicateKind.GreaterOrEqual },
            PredicateKind.LessOrEqual => this with { Kind = PredicateKind.GreaterThan },
            PredicateKind.GreaterThan => this with { Kind = PredicateKind.LessOrEqual },
            PredicateKind.GreaterOrEqual => this with { Kind = PredicateKind.LessThan },
            // Never needed: In predicates are only ever produced by CoalesceForRendering, which
            // runs after all exclude-guard negation is already done.
            PredicateKind.In => throw new NotSupportedException($"{nameof(PredicateKind.In)} predicates are never negated."),
            _ => this
        };

        /// <summary>The argument list gView's "@@if(...)" needs to check this predicate.</summary>
        public string RenderIfArgs() => Kind switch
        {
            // 1-arg: non-empty check.
            PredicateKind.NonEmpty => $"[{Field}]",
            // 2-arg equality against an empty literal ("" == "").
            PredicateKind.Empty => $"[{Field}],",
            // 2-arg equality against a specific literal.
            PredicateKind.Equals => $"[{Field}],{Value}",
            // 3-arg operator forms.
            PredicateKind.NotEquals => $"[{Field}],not,{Value}",
            PredicateKind.LessThan => $"[{Field}],lt,{Value}",
            PredicateKind.LessOrEqual => $"[{Field}],le,{Value}",
            PredicateKind.GreaterThan => $"[{Field}],gt,{Value}",
            PredicateKind.GreaterOrEqual => $"[{Field}],ge,{Value}",
            // Variable-arg form: true if the field equals any of the listed literals.
            PredicateKind.In => $"[{Field}],in,{string.Join(",", DecodeInValues(Value!))}",
            _ => throw new InvalidOperationException($"Unhandled {nameof(PredicateKind)}: {Kind}")
        };

        public bool IsNumericComparison =>
            Kind is PredicateKind.LessThan or PredicateKind.LessOrEqual or PredicateKind.GreaterThan or PredicateKind.GreaterOrEqual;

        /// <summary>
        /// Parses both operands as numbers (trying the invariant culture first, then the current
        /// culture - a plain "[Field]" placeholder is substituted via <c>object.ToString()</c>
        /// under the current culture, so e.g. a German locale can render "123,4" instead of
        /// "123.4") and compares them. Returns <see langword="false"/> if either side isn't a
        /// number - exactly mirroring <see cref="gView.Framework.Common.SimpleScriptInterpreter"/>'s
        /// "lt"/"le"/"gt"/"ge" handling.
        /// </summary>
        private static bool TryCompareNumeric(string a, string? b, out int comparison)
        {
            comparison = 0;
            if (b == null || !TryParseNumber(a, out var da) || !TryParseNumber(b, out var db))
            {
                return false;
            }
            comparison = da.CompareTo(db);
            return true;
        }

        private static bool TryParseNumber(string s, out double value) =>
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value);

        /// <summary>
        /// True if this predicate and <paramref name="other"/> (on the same field) can never both
        /// be true at once - e.g. <c>= "Schieber"</c> and <c>= "Ventil"</c>, or <c>&gt; 150</c> and
        /// <c>&lt;= 150</c>. Used to recognize when an earlier branch is automatically excluded by
        /// the current branch's own conditions, without needing a separate negated guard for it.
        /// Returns <see langword="false"/> (i.e. "can't tell, assume compatible") for anything not
        /// specifically handled below - safe because that only means a guard the algorithm didn't
        /// strictly need to add, and unnecessary/unneeded guards are still caught by
        /// <see cref="TryVerifyBranchGuards"/>, never by treating a real contradiction as none.
        /// </summary>
        public bool Contradicts(Predicate other)
        {
            if (!string.Equals(Field, other.Field, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (IsNumericComparison && other.IsNumericComparison)
            {
                bool thisIsLowerBound = Kind is PredicateKind.GreaterThan or PredicateKind.GreaterOrEqual;
                bool otherIsLowerBound = other.Kind is PredicateKind.GreaterThan or PredicateKind.GreaterOrEqual;
                if (thisIsLowerBound == otherIsLowerBound)
                {
                    return false; // two lower bounds, or two upper bounds, are never contradictory alone
                }

                var (lowerPred, upperPred) = thisIsLowerBound ? (this, other) : (other, this);
                var lo = double.Parse(lowerPred.Value!, CultureInfo.InvariantCulture);
                var hi = double.Parse(upperPred.Value!, CultureInfo.InvariantCulture);

                if (lo > hi)
                {
                    return true;
                }
                if (lo == hi)
                {
                    var lowerInclusive = lowerPred.Kind == PredicateKind.GreaterOrEqual;
                    var upperInclusive = upperPred.Kind == PredicateKind.LessOrEqual;
                    return !(lowerInclusive && upperInclusive);
                }
                return false;
            }

            return (Kind, other.Kind) switch
            {
                (PredicateKind.Empty, PredicateKind.NonEmpty) or (PredicateKind.NonEmpty, PredicateKind.Empty) => true,
                (PredicateKind.Empty, PredicateKind.Equals) or (PredicateKind.Equals, PredicateKind.Empty) => true, // Equals is always against a non-empty literal
                (PredicateKind.Equals, PredicateKind.Equals) => !string.Equals(Value, other.Value, StringComparison.Ordinal),
                (PredicateKind.Equals, PredicateKind.NotEquals) or (PredicateKind.NotEquals, PredicateKind.Equals)
                    => string.Equals(Value, other.Value, StringComparison.Ordinal),
                _ => false
            };
        }
    }

    /// <summary>
    /// Parses a VB condition such as <c>[A] &lt;&gt; "" and [B] = "Schieber"</c> into the list of
    /// AND-only predicate combinations it's equivalent to in disjunctive normal form (distributing
    /// "and" over "or": <c>A and (B or C)</c> becomes the two combinations <c>[A,B]</c> and
    /// <c>[A,C]</c>, one per branch that will later get its own guard). Standard VB precedence
    /// applies - <c>and</c> binds tighter than <c>or</c>, so <c>A and B or C</c> means
    /// <c>(A and B) or C</c> - and parentheses (nested arbitrarily deep, including a redundant
    /// pair wrapping the whole condition) always override it. See <see cref="ParseOrExpr"/> /
    /// <see cref="ParseAndExpr"/>. Returns <see langword="null"/> for anything outside that grammar
    /// (numeric comparisons other than <c>&lt;/&lt;=/&gt;/&gt;=/=/&lt;&gt;</c>, function calls, ...).
    /// </summary>
    private static List<List<Predicate>>? ParseConditionExpanded(string cond) => ParseOrExpr(cond);

    /// <summary>Parses <c>andExpr (or andExpr)*</c> into its DNF alternatives (each already AND-only).</summary>
    private static List<List<Predicate>>? ParseOrExpr(string text)
    {
        var orParts = SplitTopLevel(text, "or");

        var alternatives = new List<List<Predicate>>();
        foreach (var rawPart in orParts)
        {
            var andCombinations = ParseAndExpr(rawPart.Trim());
            if (andCombinations == null)
            {
                return null;
            }

            alternatives.AddRange(andCombinations);

            // Guard against pathological blow-up from multiplying several or/and groups together.
            if (alternatives.Count > 64)
            {
                return null;
            }
        }

        return alternatives.Count > 0 ? alternatives : null;
    }

    /// <summary>
    /// Parses <c>term (and term)*</c>, where each <c>term</c> is either a plain atom or a
    /// parenthesized sub-expression (recursing back into <see cref="ParseOrExpr"/>), into the
    /// cross-product of AND-only combinations it's equivalent to.
    /// </summary>
    private static List<List<Predicate>>? ParseAndExpr(string text)
    {
        var andParts = SplitTopLevel(text, "and");

        List<List<Predicate>> combinations = [[]];

        foreach (var rawPart in andParts)
        {
            var part = rawPart.Trim();

            List<List<Predicate>>? partAlternatives;
            if (IsFullyParenthesized(part))
            {
                partAlternatives = ParseOrExpr(part[1..^1]); // recurse: may itself be and/or/mixed
            }
            else
            {
                var atom = ParseConditionAtom(part);
                partAlternatives = atom != null ? [[atom]] : null;
            }

            if (partAlternatives == null || partAlternatives.Count == 0)
            {
                return null;
            }

            var next = new List<List<Predicate>>();
            foreach (var combo in combinations)
            {
                foreach (var alt in partAlternatives)
                {
                    next.Add([.. combo, .. alt]);
                }
            }
            combinations = next;

            if (combinations.Count > 64)
            {
                return null;
            }
        }

        return combinations;
    }

    /// <summary>Splits <paramref name="text"/> on a keyword, ignoring occurrences nested inside parentheses.</summary>
    private static List<string> SplitTopLevel(string text, string keyword)
    {
        var depthAt = new int[text.Length];
        int depth = 0;
        for (int k = 0; k < text.Length; k++)
        {
            depthAt[k] = depth;
            if (text[k] == '(') depth++;
            else if (text[k] == ')') depth--;
        }

        var parts = new List<string>();
        int start = 0;
        foreach (Match m in Regex.Matches(text, @"\b" + keyword + @"\b", RegexOptions.IgnoreCase))
        {
            if (depthAt[m.Index] == 0)
            {
                parts.Add(text[start..m.Index]);
                start = m.Index + m.Length;
            }
        }
        parts.Add(text[start..]);
        return parts;
    }

    /// <summary>True if <paramref name="s"/> is wrapped in one matched outer pair of parentheses.</summary>
    private static bool IsFullyParenthesized(string s)
    {
        if (s.Length < 2 || s[0] != '(' || s[^1] != ')')
        {
            return false;
        }

        int depth = 0;
        for (int k = 0; k < s.Length; k++)
        {
            if (s[k] == '(')
            {
                depth++;
            }
            else if (s[k] == ')')
            {
                depth--;
                if (depth == 0 && k != s.Length - 1)
                {
                    return false; // the opening paren closes before the end of the string
                }
            }
        }
        return depth == 0;
    }

    /// <summary>
    /// Parses one condition atom: <c>[FIELD] (&lt;&gt;|=|!=|==) "Value"</c> (an empty literal maps
    /// to a plain non-empty/empty check, a non-empty one to equals/not-equals; <c>==</c>/<c>!=</c>
    /// are Python's spellings of <c>=</c>/<c>&lt;&gt;</c>), the same against an unquoted number, a
    /// numeric comparison <c>FIELD (&lt;|&lt;=|&gt;|&gt;=) Number</c> - where <c>FIELD</c> may
    /// optionally be wrapped in a no-op numeric/string cast, e.g. <c>int([Field]) &gt; 1</c> - in
    /// either operand order, or a null check: VB's <c>IsNull([Field])</c>/<c>Not IsNull([Field])</c>
    /// or Python's <c>[Field] is None</c>/<c>[Field] is not None</c>.
    /// </summary>
    private static Predicate? ParseConditionAtom(string atom)
    {
        var nullMatch = Regex.Match(atom, @"^IsNull\s*\(\s*\[(?<field>[^\]]+)\]\s*\)$", RegexOptions.IgnoreCase);
        if (nullMatch.Success)
        {
            return new Predicate(nullMatch.Groups["field"].Value.Trim(), PredicateKind.Empty);
        }
        var notNullMatch = Regex.Match(atom, @"^Not\s+IsNull\s*\(\s*\[(?<field>[^\]]+)\]\s*\)$", RegexOptions.IgnoreCase);
        if (notNullMatch.Success)
        {
            return new Predicate(notNullMatch.Groups["field"].Value.Trim(), PredicateKind.NonEmpty);
        }
        var isNoneMatch = Regex.Match(atom, @"^\[(?<field>[^\]]+)\]\s*is\s+None$", RegexOptions.IgnoreCase);
        if (isNoneMatch.Success)
        {
            return new Predicate(isNoneMatch.Groups["field"].Value.Trim(), PredicateKind.Empty);
        }
        var isNotNoneMatch = Regex.Match(atom, @"^\[(?<field>[^\]]+)\]\s*is\s+not\s+None$", RegexOptions.IgnoreCase);
        if (isNotNoneMatch.Success)
        {
            return new Predicate(isNotNoneMatch.Groups["field"].Value.Trim(), PredicateKind.NonEmpty);
        }

        const string valuePattern = @"""(?<value>(?:[^""]|"""")*)""";
        const string eqOpPattern = "(?<op><>|!=|==|=)"; // longer tokens first so "==" doesn't match as "=" + "="

        var m = Regex.Match(atom, $@"^\[(?<field>[^\]]+)\]\s*{eqOpPattern}\s*{valuePattern}$");
        if (!m.Success)
        {
            m = Regex.Match(atom, $@"^{valuePattern}\s*{eqOpPattern}\s*\[(?<field>[^\]]+)\]$");
        }
        if (m.Success)
        {
            var field = m.Groups["field"].Value.Trim();
            var isNotEqual = m.Groups["op"].Value is "<>" or "!=";
            var value = m.Groups["value"].Value.Replace("\"\"", "\""); // VB escapes a literal quote as ""

            if (value.Length == 0)
            {
                return new Predicate(field, isNotEqual ? PredicateKind.NonEmpty : PredicateKind.Empty);
            }

            // gView's "@@if([Field],Value)" splits its arguments on a bare "," with no escaping,
            // so a "," inside the literal would be silently mis-parsed at runtime (same reasoning
            // as the Replace(...) chain guard) - bail rather than risk generating a subtly wrong
            // script for this (rare) case.
            if (value.Contains(','))
            {
                return null;
            }

            return new Predicate(field, isNotEqual ? PredicateKind.NotEquals : PredicateKind.Equals, value);
        }

        const string numberPattern = @"(?<value>-?\d+(?:\.\d+)?)";
        // A field reference, optionally wrapped in one no-op numeric/string cast - e.g.
        // "int([Field])" - which only changes the runtime type, not which field is compared.
        var fieldOrCastPattern = $@"(?:\[(?<field>[^\]]+)\]|(?:{NumericCastNames})\s*\(\s*\[(?<field>[^\]]+)\]\s*\))";

        // Equality/inequality against an unquoted number, e.g. "[TYP] = 200" - reuses the same
        // Equals/NotEquals predicate kinds as the quoted-string form (a plain string comparison
        // against the number's own text is enough for equality/inequality purposes; unlike
        // "<"/"<="/">"/">=", there's no interval reasoning that would need real numeric parsing).
        var eqm = Regex.Match(atom, $@"^{fieldOrCastPattern}\s*{eqOpPattern}\s*{numberPattern}$");
        if (!eqm.Success)
        {
            eqm = Regex.Match(atom, $@"^{numberPattern}\s*{eqOpPattern}\s*{fieldOrCastPattern}$");
        }
        if (eqm.Success)
        {
            var eqField = eqm.Groups["field"].Value.Trim();
            var eqValue = eqm.Groups["value"].Value;
            var eqIsNotEqual = eqm.Groups["op"].Value is "<>" or "!="; // operand order doesn't matter for =/<>, unlike </>
            return new Predicate(eqField, eqIsNotEqual ? PredicateKind.NotEquals : PredicateKind.Equals, eqValue);
        }

        var nm = Regex.Match(atom, $@"^{fieldOrCastPattern}\s*(?<op><=|>=|<|>)\s*{numberPattern}$");
        var reversed = false;
        if (!nm.Success)
        {
            nm = Regex.Match(atom, $@"^{numberPattern}\s*(?<op><=|>=|<|>)\s*{fieldOrCastPattern}$");
            reversed = true;
        }
        if (!nm.Success)
        {
            return null;
        }

        var numField = nm.Groups["field"].Value.Trim();
        var numValue = nm.Groups["value"].Value;

        // In reversed order ("100 >= [Field]" means "[Field] <= 100") the operator flips.
        var kind = (nm.Groups["op"].Value, reversed) switch
        {
            ("<", false) or (">", true) => PredicateKind.LessThan,
            ("<=", false) or (">=", true) => PredicateKind.LessOrEqual,
            (">", false) or ("<", true) => PredicateKind.GreaterThan,
            (">=", false) or ("<=", true) => PredicateKind.GreaterOrEqual,
            _ => throw new InvalidOperationException("Unreachable: regex only matches <, <=, >, >=.")
        };

        return new Predicate(numField, kind, numValue);
    }

    /// <summary>
    /// Tries to reduce an If/ElseIf/[Else] branch set to gView's conditional mini-script.
    /// </summary>
    /// <remarks>
    /// Each branch's own text is reproduced verbatim (never merged with another branch's text),
    /// wrapped in a nested <c>@@if</c> guard built from that branch's position in the original
    /// VB chain:
    /// <list type="bullet">
    /// <item>one <c>@@if(...)</c> per predicate the branch's own condition requires true, and</item>
    /// <item>one <c>@@if(...)</c> for the <em>negation</em> of every predicate required by an
    /// <em>earlier</em> branch but not (exactly) by this one - so an earlier, more specific branch
    /// that would have matched first in VB still "wins" here too.</item>
    /// </list>
    /// This needs <see cref="gView.Framework.Common.SimpleScriptInterpreter"/> to support properly
    /// nested <c>@@if</c>/<c>@@endif</c> (an AND of every enclosing condition), which it does.
    /// The result is only accepted after <see cref="TryVerifyBranchGuards"/> confirms it produces
    /// identical text to the original VB chain for every combination of field values worth
    /// distinguishing - including the "no branch matches" case, where VB implicitly returns an
    /// empty string. Anything that still doesn't verify (e.g. a condition that's really an
    /// <c>or</c> of several checks, which can't be expressed as this kind of AND-only guard) is
    /// rejected.
    /// </remarks>
    /// <summary>
    /// For each field referenced in <paramref name="allPredicates"/>, builds the values worth
    /// exercising when exhaustively verifying a generated conditional script: "" (empty), every
    /// literal it's compared against, probe values between/around any numeric-comparison
    /// thresholds for that field (so e.g. "&gt; 100 and &lt;= 150" actually gets tested with a
    /// value strictly between 100 and 150, not just at the thresholds themselves), and one
    /// synthetic "some other value" catch-all. Returns <see langword="false"/> (a pathological
    /// blow-up guard) if there are too many fields, thresholds, or combinations to check -
    /// real-world label expressions compare a handful of fields against a handful of values.
    /// Shared by both the If/ElseIf/Else guard algorithm and the independent-append-chain one.
    /// </summary>
    private static bool TryBuildFieldDomains(
        List<Predicate> allPredicates,
        out List<string> fields,
        out Dictionary<string, List<string>> fieldDomains)
    {
        fieldDomains = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var predicate in allPredicates)
        {
            if (!fieldDomains.TryGetValue(predicate.Field, out var domain))
            {
                fieldDomains[predicate.Field] = domain = [""];
            }
            if (predicate.Value != null && !domain.Contains(predicate.Value, StringComparer.Ordinal))
            {
                domain.Add(predicate.Value);
            }
        }

        fields = fieldDomains.Keys.ToList();

        if (fields.Count > 8)
        {
            return false;
        }

        foreach (var field in fields)
        {
            var domain = fieldDomains[field];

            var thresholds = allPredicates
                .Where(p => string.Equals(p.Field, field, StringComparison.OrdinalIgnoreCase) && p.IsNumericComparison)
                .Select(p => double.Parse(p.Value!, CultureInfo.InvariantCulture))
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            if (thresholds.Count == 0)
            {
                continue;
            }
            if (thresholds.Count > 20)
            {
                return false;
            }

            var probes = new List<double> { thresholds[0] - 1 };
            for (int i = 0; i < thresholds.Count; i++)
            {
                probes.Add(thresholds[i]);
                if (i + 1 < thresholds.Count)
                {
                    probes.Add((thresholds[i] + thresholds[i + 1]) / 2.0);
                }
            }
            probes.Add(thresholds[^1] + 1);

            foreach (var probe in probes)
            {
                var s = probe.ToString(CultureInfo.InvariantCulture);
                if (!domain.Contains(s, StringComparer.Ordinal))
                {
                    domain.Add(s);
                }
            }
        }

        foreach (var field in fields)
        {
            var domain = fieldDomains[field];
            var other = "OTHER";
            for (int n = 0; domain.Contains(other, StringComparer.Ordinal); n++)
            {
                other = $"OTHER{n}";
            }
            domain.Add(other);
        }

        long comboCount = 1;
        foreach (var field in fields)
        {
            comboCount *= fieldDomains[field].Count;
            if (comboCount > 20_000)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Walks every combination of field values across <paramref name="fields"/>'s domains
    /// (Cartesian product), calling <paramref name="check"/> once per combination; stops early
    /// and returns <see langword="false"/> as soon as one combination fails.
    /// </summary>
    private static bool ForAllCombinations(
        List<string> fields,
        Dictionary<string, List<string>> fieldDomains,
        Func<Dictionary<string, string>, bool> check)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        return Walk(0);

        bool Walk(int fieldIndex)
        {
            if (fieldIndex == fields.Count)
            {
                return check(values);
            }

            var field = fields[fieldIndex];
            foreach (var value in fieldDomains[field])
            {
                values[field] = value;
                if (!Walk(fieldIndex + 1))
                {
                    return false;
                }
            }
            values.Remove(field);
            return true;
        }
    }

    private static bool TryBuildConditionalScript(List<Branch> branches, out ConversionResult? result)
    {
        result = null;

        var allPredicates = branches.SelectMany(b => b.Conditions).ToList();
        if (!TryBuildFieldDomains(allPredicates, out var fields, out var fieldDomains))
        {
            return false;
        }

        // For each branch, every *earlier* branch must be prevented from also matching - otherwise
        // that earlier (more specific) branch would have matched first in the original VB
        // If/ElseIf chain. An earlier branch is excluded for free (no guard needed) if any single
        // one of its own conditions already directly contradicts one of the current branch's own
        // conditions (e.g. an earlier "<= 150" branch can never also match once we already require
        // "> 150") - see Predicate.Contradicts. Otherwise, the earlier branch's conditions not
        // already implied by the current branch's own conditions must be negated; that only works
        // as a single AND-only guard when there's exactly one such condition; excluding "at least
        // one of several is false" would need real OR, which isn't expressible here, so that's
        // rejected rather than built incorrectly (see AprxLabelExpressionParserConditionalTests
        // for the range-bucketing case this specifically fixes).
        var excludePerBranch = new List<List<Predicate>>();
        var earlierBranches = new List<List<Predicate>>();
        foreach (var branch in branches)
        {
            var ownConditions = new HashSet<Predicate>(branch.Conditions);
            var exclude = new List<Predicate>();

            foreach (var earlierConditions in earlierBranches)
            {
                if (earlierConditions.Any(ep => branch.Conditions.Any(bp => bp.Contradicts(ep))))
                {
                    continue; // this earlier branch can never also fire - nothing to add
                }

                var extra = earlierConditions.Where(ep => !ownConditions.Contains(ep)).ToList();
                if (extra.Count != 1)
                {
                    // 0: the earlier branch's conditions are a subset of ours, so it always fires
                    //    whenever we do - we could never actually be reached; not expressible here.
                    // 2+: would need "at least one of these is false" (OR), not expressible as a
                    //    single AND-only guard.
                    return false;
                }

                var negated = extra[0].Negate();
                if (!exclude.Contains(negated))
                {
                    exclude.Add(negated);
                }
            }

            excludePerBranch.Add(exclude);
            earlierBranches.Add(branch.Conditions);
        }

        bool VerifyCombination(Dictionary<string, string> values) => VerifyOneCombination(branches, branches, excludePerBranch, values);
        if (!ForAllCombinations(fields, fieldDomains, VerifyCombination))
        {
            return false;
        }

        // Readability pass: several branches that agree on everything (their other conditions,
        // their exclude-guards, and their output) except one field's specific Equals value collapse
        // into a single branch with an "in" check over the union of those values - this is purely
        // cosmetic (fewer, shorter "@@if" blocks) and never changes behaviour, so the merged result
        // is exhaustively re-verified against the original (unmerged, already-verified) branches
        // before being used; any verification failure silently falls back to the unmerged rendering
        // rather than risk wrong output. See CoalesceForRendering.
        var (mergedBranches, mergedExcludes) = CoalesceForRendering(branches, excludePerBranch);
        if (mergedBranches.Count < branches.Count)
        {
            bool VerifyMerged(Dictionary<string, string> values) => VerifyOneCombination(branches, mergedBranches, mergedExcludes, values);
            if (ForAllCombinations(fields, fieldDomains, VerifyMerged))
            {
                result = new ConversionResult(RenderConditionalScript(mergedBranches, mergedExcludes), IsConditional: true);
                return true;
            }
        }

        result = new ConversionResult(RenderConditionalScript(branches, excludePerBranch), IsConditional: true);
        return true;
    }

    /// <summary>
    /// Merges branches that are otherwise identical (same output, same other conditions, same
    /// exclude-guards) but disagree on a single field's Equals value into one branch whose
    /// condition on that field becomes an <see cref="PredicateKind.In"/> check over the union of
    /// those values - e.g. four branches that only differ by <c>[SA1] = "N"</c> / <c>"M"</c> /
    /// <c>"H"</c> / <c>"F"</c> (everything else identical) collapse into one
    /// <c>[SA1] in {"N","M","H","F"}</c> branch. Runs to a fixed point, so a merge that makes two
    /// further branches identical (e.g. after their own varying field was already merged away) can
    /// itself be merged in a later pass - this is how the classic "two fields, each independently
    /// enumerated" case reduces to just a couple of branches. Only ever merges on a field where
    /// every candidate branch has exactly one condition (an <see cref="PredicateKind.Equals"/> or
    /// already-merged <see cref="PredicateKind.In"/>) - fields with a numeric range (two conditions,
    /// e.g. a lower and upper bound) are never touched, and branches with more than one condition on
    /// the same field are skipped entirely for safety. Purely a rendering-time simplification: the
    /// result is always re-verified by the caller against the original branches before use.
    /// </summary>
    private static (List<Branch> Branches, List<List<Predicate>> Exclude) CoalesceForRendering(
        List<Branch> branches, List<List<Predicate>> excludePerBranch)
    {
        var workBranches = new List<Branch>(branches);
        var workExcludes = new List<List<Predicate>>(excludePerBranch);

        bool mergedAny;
        do
        {
            mergedAny = false;

            for (int i = 0; i < workBranches.Count && !mergedAny; i++)
            {
                if (!TryGetFieldMap(workBranches[i].Conditions, out var mapI))
                {
                    continue;
                }

                foreach (var mergeField in mapI.Keys.ToList())
                {
                    if (mapI[mergeField].Kind is not (PredicateKind.Equals or PredicateKind.In))
                    {
                        continue;
                    }

                    var group = new List<(int Index, Predicate MergeFieldPredicate)>();
                    for (int j = 0; j < workBranches.Count; j++)
                    {
                        if (!TryGetFieldMap(workBranches[j].Conditions, out var mapJ))
                        {
                            continue;
                        }
                        if (mapJ.Count != mapI.Count || !mapI.Keys.All(mapJ.ContainsKey))
                        {
                            continue;
                        }
                        var mergeFieldPredicate = mapJ[mergeField];
                        if (mergeFieldPredicate.Kind is not (PredicateKind.Equals or PredicateKind.In))
                        {
                            continue;
                        }
                        if (mapI.Keys.Any(f => f != mergeField && !mapI[f].Equals(mapJ[f])))
                        {
                            continue;
                        }
                        if (!workBranches[j].Terms.SequenceEqual(workBranches[i].Terms))
                        {
                            continue;
                        }
                        if (!PredicateListEquals(workExcludes[j], workExcludes[i]))
                        {
                            continue;
                        }
                        group.Add((j, mergeFieldPredicate));
                    }

                    if (group.Count < 2)
                    {
                        continue;
                    }

                    var mergedValues = group
                        .SelectMany(g => ValuesOf(g.MergeFieldPredicate))
                        .Distinct(StringComparer.Ordinal)
                        .ToList();
                    var mergedPredicate = mergedValues.Count == 1
                        ? new Predicate(mergeField, PredicateKind.Equals, mergedValues[0])
                        : new Predicate(mergeField, PredicateKind.In, EncodeInValues(mergedValues));

                    int insertAt = group.Min(g => g.Index);
                    var template = workBranches[insertAt];
                    var newConditions = template.Conditions
                        .Select(p => string.Equals(p.Field, mergeField, StringComparison.OrdinalIgnoreCase) ? mergedPredicate : p)
                        .ToList();
                    var newBranch = template with { Conditions = newConditions };
                    var newExclude = workExcludes[insertAt];

                    foreach (var idx in group.Select(g => g.Index).OrderByDescending(x => x))
                    {
                        workBranches.RemoveAt(idx);
                        workExcludes.RemoveAt(idx);
                    }

                    workBranches.Insert(insertAt, newBranch);
                    workExcludes.Insert(insertAt, newExclude);

                    mergedAny = true;
                    break;
                }
            }
        } while (mergedAny);

        return (workBranches, workExcludes);
    }

    /// <summary>
    /// Builds a Field -> Predicate map for one branch's conditions, or returns
    /// <see langword="false"/> if any field has more than one condition (e.g. a numeric range's
    /// lower and upper bound) - such branches are left out of <see cref="CoalesceForRendering"/>
    /// entirely, since "the one field that differs" isn't well-defined for them.
    /// </summary>
    private static bool TryGetFieldMap(List<Predicate> conditions, out Dictionary<string, Predicate> map)
    {
        map = new Dictionary<string, Predicate>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in conditions)
        {
            if (!map.TryAdd(p.Field, p))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>Order-independent equality of two predicate lists (used to compare exclude-guards).</summary>
    private static bool PredicateListEquals(List<Predicate> a, List<Predicate> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }
        var remaining = new List<Predicate>(b);
        foreach (var pa in a)
        {
            var idx = remaining.FindIndex(pb => pb.Equals(pa));
            if (idx < 0)
            {
                return false;
            }
            remaining.RemoveAt(idx);
        }
        return true;
    }

    private static IEnumerable<string> ValuesOf(Predicate p) =>
        p.Kind == PredicateKind.In ? DecodeInValues(p.Value!) : new[] { p.Value! };

    // Separator for PredicateKind.In's multi-value Predicate.Value - a control character that can
    // never appear in a parsed VB/Python/Arcade literal, so it's always safe to split back apart.
    private const char InValueSeparator = '\u0001';

    private static string EncodeInValues(IEnumerable<string> values) => string.Join(InValueSeparator, values);

    private static string[] DecodeInValues(string encoded) => encoded.Split(InValueSeparator);

    /// <summary>
    /// Checks, for one combination of field values, that the per-branch guards (positive =
    /// <paramref name="branches"/>' own conditions, negative = <paramref name="excludePerBranch"/>)
    /// reproduce exactly the same text as the original VB If/ElseIf/[Else] chain - computed from
    /// <paramref name="groundTruthBranches"/> - would, including the "no branch matches" case,
    /// where VB implicitly returns an empty string. Called once per combination by
    /// <see cref="ForAllCombinations"/>. <paramref name="groundTruthBranches"/> and
    /// <paramref name="branches"/> are the same list for the initial (unmerged) verification in
    /// <see cref="TryBuildConditionalScript"/>; they differ when re-verifying a
    /// <see cref="CoalesceForRendering"/> merge, where <paramref name="groundTruthBranches"/> stays
    /// the original, already-verified branches and <paramref name="branches"/>/
    /// <paramref name="excludePerBranch"/> are the merged ones being checked against it.
    /// </summary>
    private static bool VerifyOneCombination(
        List<Branch> groundTruthBranches,
        List<Branch> branches,
        List<List<Predicate>> excludePerBranch,
        Dictionary<string, string> values)
    {
        string ValueOf(string field) => values.TryGetValue(field, out var v) ? v : "";
        bool AllTrue(IEnumerable<Predicate> predicates) => predicates.All(p => p.Evaluate(ValueOf(p.Field)));

        // What the original VB If/ElseIf/[Else] chain would produce (first branch whose
        // conditions are all satisfied; no match => empty, matching VB's default).
        var vbTerms = groundTruthBranches.FirstOrDefault(b => AllTrue(b.Conditions))?.Terms ?? [];
        var vbText = RenderTermsForComparison(vbTerms);

        // What gView's SimpleScriptInterpreter would actually produce: every branch whose guard
        // evaluates true contributes its own lines, joined the same way the real interpreter
        // joins included lines (a separator only once something's been appended).
        var sb = new StringBuilder();
        for (int i = 0; i < branches.Count; i++)
        {
            if (!AllTrue(branches[i].Conditions) || !AllTrue(excludePerBranch[i]))
            {
                continue;
            }

            foreach (var line in SplitIntoLines(branches[i].Terms))
            {
                if (sb.Length > 0)
                {
                    sb.Append('\n');
                }
                sb.Append(RenderTermsForComparison(line));
            }
        }

        return string.Equals(vbText, sb.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Renders terms the same way as <see cref="RenderTerms"/>, but normalizes line breaks to
    /// <c>'\n'</c> (rather than <see cref="Environment.NewLine"/>) so results are directly
    /// comparable regardless of platform - used only for the equivalence checks above, never
    /// for the actual generated expression.
    /// </summary>
    private static string RenderTermsForComparison(IEnumerable<Term> terms)
    {
        var sb = new StringBuilder();
        foreach (var term in terms)
        {
            switch (term)
            {
                case LiteralTerm lit: sb.Append(lit.Text); break;
                case FieldTerm f:
                    sb.Append('[').Append(f.Name);
                    if (f.Format != null)
                    {
                        sb.Append(':').Append(f.Format);
                    }
                    sb.Append(']');
                    break;
                case NewlineTerm: sb.Append('\n'); break;
            }
        }
        return sb.ToString();
    }

    /// <summary>Builds the gView "@@start/@@if/@@endif/@@end" mini-script for the given branches/guards.</summary>
    private static string RenderConditionalScript(List<Branch> branches, List<List<Predicate>> excludePerBranch)
    {
        var sb = new StringBuilder();
        sb.Append("@@start").Append(Environment.NewLine);

        for (int i = 0; i < branches.Count; i++)
        {
            int depth = 0;
            foreach (var predicate in branches[i].Conditions)
            {
                sb.Append($"@@if({predicate.RenderIfArgs()})").Append(Environment.NewLine);
                depth++;
            }
            foreach (var predicate in excludePerBranch[i])
            {
                sb.Append($"@@if({predicate.RenderIfArgs()})").Append(Environment.NewLine);
                depth++;
            }

            foreach (var line in SplitIntoLines(branches[i].Terms))
            {
                sb.Append(RenderTerms(line)).Append(Environment.NewLine);
            }

            for (int d = 0; d < depth; d++)
            {
                sb.Append("@@endif").Append(Environment.NewLine);
            }
        }

        sb.Append("@@end");
        return sb.ToString();
    }

    // -----------------------------------------------------------------------
    // Term tokenizer: string literals, [Field] refs, vbNewLine/vbCrLf/vbTab, "&"
    // -----------------------------------------------------------------------

    private abstract record Term;
    private sealed record LiteralTerm(string Text) : Term;
    /// <summary><paramref name="Format"/>, when set, is a .NET numeric format string (e.g. "F2")
    /// applied via gView's existing "[Field:Format]" placeholder syntax - see
    /// <see cref="TryParseRoundCall"/>.</summary>
    private sealed record FieldTerm(string Name, string? Format = null) : Term;
    private sealed record NewlineTerm : Term;

    /// <summary>
    /// ArcGIS Pro / ArcMap "text formatting tags" for rich text in labels and annotations, e.g.
    /// <c>&lt;CLR red='255' green='0' blue='0'&gt;...&lt;/CLR&gt;</c> for per-run color, or
    /// <c>&lt;BOL&gt;...&lt;/BOL&gt;</c> for bold. gView's label text symbols have no per-run
    /// formatting, so both the opening (with any attributes) and closing tag are stripped
    /// entirely wherever they appear inside a string literal - only their content stays.
    /// </summary>
    private static readonly Regex RichTextTag = new(
        @"</?(?:AND|ACP|BOL|CHR|CHRIDX|CLR|CPS|FNT|ITA|SUB|SUP|UND|VER|VOF)\b[^<>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Tokenizes a VB (or Python, when <paramref name="pythonEscapes"/> is set) concatenation
    /// expression (string literals, <c>[Field]</c> references, <c>vbNewLine</c>/<c>vbCrLf</c>/
    /// <c>vbLf</c>/<c>vbCr</c>/<c>vbTab</c> constants, <c>round(...)</c> calls (see
    /// <see cref="TryParseRoundCall"/>), joined with <c>&amp;</c>) into a term list. Returns
    /// <see langword="false"/> for anything else (other function calls, other operators, bare
    /// numbers, ...).
    /// </summary>
    /// <param name="pythonEscapes">
    /// When set, a backslash inside a string literal is decoded as a Python escape sequence
    /// (<c>\n</c> becomes a line break, <c>\t</c> a tab, <c>\\</c>/<c>\'</c>/<c>\"</c> the literal
    /// character, anything else is kept as-is) - Python's <c>'\n'</c> really is a one-character
    /// newline string, unlike VBScript, where a literal has no escape mechanism at all and a
    /// backslash is always just an ordinary character (e.g. inside a file path). Only set this for
    /// text known to come from a Python-flavored <c>def</c> function body.
    /// </param>
    private static bool TryTokenizeConcatExpression(string expr, out List<Term> terms, bool pythonEscapes = false)
    {
        terms = [];
        if (string.IsNullOrEmpty(expr))
        {
            return true;
        }

        var list = new List<Term>();
        int i = 0, n = expr.Length;
        var pendingLiteral = new StringBuilder();

        void FlushLiteral()
        {
            if (pendingLiteral.Length > 0)
            {
                // Strip ArcGIS Pro's rich-text formatting tags (e.g. "<CLR red='255' .../>...</CLR>"
                // for per-run colored text) - gView's label text symbols render plain text only,
                // so these tags would otherwise show up literally in the label. Only their content
                // is kept; nothing about the surrounding text or field placeholders is touched.
                var text = RichTextTag.Replace(pendingLiteral.ToString(), "");
                if (text.Length > 0)
                {
                    list.Add(new LiteralTerm(text));
                }
                pendingLiteral.Clear();
            }
        }

        while (i < n)
        {
            char c = expr[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (c == '"' || c == '\'')
            {
                // ArcGIS Pro's label-expression parser accepts both '"..."' and "'...'" as string
                // literals (unlike strict VBScript, where "'" only starts a comment) - "''"/""""
                // escapes a literal quote of the matching kind inside the literal.
                var quote = c;
                i++;
                var closed = false;
                while (i < n)
                {
                    if (pythonEscapes && expr[i] == '\\' && i + 1 < n)
                    {
                        var next = expr[i + 1];
                        switch (next)
                        {
                            case 'n':
                                FlushLiteral();
                                list.Add(new NewlineTerm());
                                break;
                            case 't':
                                pendingLiteral.Append('\t');
                                break;
                            case '\\':
                            case '\'':
                            case '"':
                                pendingLiteral.Append(next);
                                break;
                            default:
                                // Unknown escape - keep both characters literally, same as
                                // Python does for anything it doesn't specifically recognize.
                                pendingLiteral.Append('\\').Append(next);
                                break;
                        }
                        i += 2;
                        continue;
                    }
                    if (expr[i] == quote)
                    {
                        if (i + 1 < n && expr[i + 1] == quote)
                        {
                            pendingLiteral.Append(quote);
                            i += 2;
                            continue;
                        }
                        i++; // closing quote
                        closed = true;
                        break;
                    }
                    pendingLiteral.Append(expr[i]);
                    i++;
                }
                if (!closed)
                {
                    return false; // unterminated string literal - malformed, don't guess
                }
                continue;
            }

            if (c == '[')
            {
                int close = expr.IndexOf(']', i + 1);
                if (close < 0)
                {
                    return false; // malformed
                }
                FlushLiteral();
                list.Add(new FieldTerm(expr.Substring(i + 1, close - i - 1).Trim()));
                i = close + 1;
                continue;
            }

            if (c == '$' && i + 9 <= n && string.Compare(expr, i, "$feature.", 0, 9, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Arcade's field-access syntax, e.g. "$feature.MELD_TXT" - treated the same as
                // gView's "[MELD_TXT]" placeholder. Only a bare field access is supported (no
                // further Arcade expression syntax).
                int start = i + 9;
                int end = start;
                while (end < n && (char.IsLetterOrDigit(expr[end]) || expr[end] == '_'))
                {
                    end++;
                }
                if (end == start)
                {
                    return false; // "$feature." not followed by a field name
                }
                FlushLiteral();
                var accessed = expr[start..end];
                // "$feature.Length"/"$feature.Area" are Arcade's geometry accessors, not a real
                // attribute field - map onto the same reserved pseudo-field gView's runtime
                // computes from the feature's geometry (see GeometryLengthFieldName/
                // GeometryAreaFieldName). Without this special case, "$feature.Length" would
                // silently become FieldTerm("Length"), as if a real field named "Length" existed.
                if (accessed.Equals("Length", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(new FieldTerm(GeometryLengthFieldName));
                }
                else if (accessed.Equals("Area", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(new FieldTerm(GeometryAreaFieldName));
                }
                else
                {
                    list.Add(new FieldTerm(accessed));
                }
                i = end;
                continue;
            }

            if (c == '&' || c == '+' || c == '%')
            {
                // Concatenation operator. "+" is VBScript's (and Python's) other string-
                // concatenation operator, alongside "&". "%" isn't a real VB operator at all
                // (VBScript's modulo keyword is "Mod", not "%") - but since we never accept
                // numeric literals, within the subset this tokenizer understands it can't mean
                // anything else either, so it's tolerated the same way (a likely "&" typo in
                // real-world source data, common enough to be worth not rejecting outright).
                // Just a separator between terms either way, dropped like "&".
                i++;
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < n && (char.IsLetterOrDigit(expr[i]) || expr[i] == '_'))
                {
                    i++;
                }
                var word = expr.Substring(start, i - start);
                switch (word.ToLowerInvariant())
                {
                    case "vbnewline":
                    case "vbnewnline": // common source typo (extra "n") - tolerated, not guessed
                    case "vbcrlf":
                    case "vblf":
                    case "vbcr":
                        FlushLiteral();
                        list.Add(new NewlineTerm());
                        break;
                    case "vbtab":
                        pendingLiteral.Append('\t');
                        break;
                    case "round":
                        if (!TryParseRoundCall(expr, ref i, out var roundedField))
                        {
                            return false;
                        }
                        FlushLiteral();
                        list.Add(roundedField);
                        break;
                    case "float":
                    case "cdbl":
                    case "cdec":
                    case "csng":
                    case "val":
                    case "cint":
                    case "clng":
                    case "int":  // Python
                    case "str":  // Python
                    case "repr": // Python
                        // A numeric/string cast wrapping a field reference, standing alone (not
                        // inside round(...)) - it only changes the runtime type, not which field
                        // is shown, so it's a no-op for our purposes: "str([A])" becomes "[A]".
                        if (!TryParsePassthroughCast(expr, ref i, out var castedField))
                        {
                            return false;
                        }
                        FlushLiteral();
                        list.Add(castedField);
                        break;
                    case "length":
                    case "area":
                        // Arcade's "Length($feature)"/"Area($feature)" - the geometry-derived
                        // counterpart to "$feature.Length"/"$feature.Area" above. See
                        // GeometryLengthFieldName/GeometryAreaFieldName.
                        if (!TryParseFeatureGeometryCall(expr, ref i))
                        {
                            return false;
                        }
                        FlushLiteral();
                        list.Add(new FieldTerm(word.ToLowerInvariant() == "length" ? GeometryLengthFieldName : GeometryAreaFieldName));
                        break;
                    default:
                        return false; // unknown identifier / function call - unsupported
                }
                continue;
            }

            return false; // operators, parentheses, numbers, ... - unsupported
        }

        FlushLiteral();
        terms = list;
        return true;
    }

    /// <summary>
    /// Parses a <c>round(&lt;value&gt;, &lt;digits&gt;)</c> call starting at the <c>(</c> right
    /// after the already-consumed "round" keyword (<paramref name="i"/> points just past it),
    /// where <c>&lt;value&gt;</c> is a bare <c>[Field]</c> reference or a single numeric-cast call
    /// wrapping one (<c>float(...)</c>, <c>cdbl(...)</c>, <c>cdec(...)</c>, <c>csng(...)</c>,
    /// <c>val(...)</c>, <c>cint(...)</c>, <c>clng(...)</c> - VB/VBScript's numeric conversion
    /// functions, which don't change which field gets displayed). Rounding a field's value to N
    /// decimal digits is exactly what gView's existing <c>[Field:F2]</c>-style placeholder
    /// format already does (see <see cref="gView.Framework.Cartography.Rendering.Exntensions.ExpressionExtensions.EvaluateExpression"/>)
    /// - <em>provided</em> the field is actually stored as a numeric type in the gView feature
    /// class; for a text-typed field the format is silently ignored (a pre-existing behaviour of
    /// that formatter, not introduced here), so this is worth spot-checking against real data
    /// after conversion.
    /// </summary>
    private static bool TryParseRoundCall(string expr, ref int i, out FieldTerm term)
    {
        term = null!;
        int n = expr.Length;
        int j = i;
        while (j < n && char.IsWhiteSpace(expr[j]))
        {
            j++;
        }
        if (j >= n || expr[j] != '(')
        {
            return false;
        }

        int depth = 0;
        int openParen = j;
        int closeParen = -1;
        for (int k = j; k < n; k++)
        {
            if (expr[k] == '(')
            {
                depth++;
            }
            else if (expr[k] == ')')
            {
                depth--;
                if (depth == 0)
                {
                    closeParen = k;
                    break;
                }
            }
        }
        if (closeParen < 0)
        {
            return false; // unbalanced parens
        }

        var inner = expr.Substring(openParen + 1, closeParen - openParen - 1);

        // Split on the last top-level (paren-depth 0 within `inner`) comma: "<value>, <digits>".
        int splitAt = -1;
        int innerDepth = 0;
        for (int k = 0; k < inner.Length; k++)
        {
            if (inner[k] == '(')
            {
                innerDepth++;
            }
            else if (inner[k] == ')')
            {
                innerDepth--;
            }
            else if (inner[k] == ',' && innerDepth == 0)
            {
                splitAt = k;
            }
        }
        if (splitAt < 0)
        {
            return false; // round() needs both a value and a digit count
        }

        var valueText = inner[..splitAt].Trim();
        var digitsText = inner[(splitAt + 1)..].Trim();

        if (!Regex.IsMatch(digitsText, @"^\d+$") || !int.TryParse(digitsText, out var digits) || digits > 15)
        {
            return false;
        }

        var fieldName = ExtractFieldFromNumericValue(valueText);
        if (fieldName == null)
        {
            return false;
        }

        term = new FieldTerm(fieldName, "F" + digits);
        i = closeParen + 1;
        return true;
    }

    /// <summary>
    /// Numeric/string cast function names (VB and Python) that don't change which field is
    /// displayed, just its runtime type - a no-op for our purposes.
    /// </summary>
    private const string NumericCastNames = "float|cdbl|cdec|csng|val|cint|clng|int|str|repr";

    /// <summary>
    /// Reserved pseudo-field names for ArcGIS Pro's <c>Length($feature)</c>/<c>$feature.Length</c>
    /// and <c>Area($feature)</c>/<c>$feature.Area</c> - resolved at label-render time from the
    /// feature's actual geometry (see <see cref="gView.Framework.Cartography.Rendering.SimpleLabelRenderer"/>),
    /// not from an attribute field. Deliberately spelled to echo the Arcade syntax that produces
    /// it (<c>[$feature.length]</c> reads like "$feature.Length" applied to the feature) rather
    /// than gView's usual "[Field]" look. The leading "$" (and the "." - never valid in a real
    /// column/field name either) makes this impossible to collide with a real attribute field:
    /// virtually no database/shapefile backend allows either character in a column name, so -
    /// unlike reusing gView's existing "Shape_Length"/"Shape_Area" auto-field convention, which
    /// real schemas can and do have as genuine, independently-populated columns - there's no need
    /// to prefer a same-named real field over the computed value; the computed value is always
    /// unambiguously what's meant. Only "." is used, never ":", since gView's own placeholder
    /// syntax already gives ":" a reserved meaning (the "[Field:Format]" format separator - see
    /// <see cref="gView.Framework.Cartography.Rendering.Exntensions.ExpressionExtensions.EvaluateExpression"/>);
    /// a "$feature:length" spelling would be misparsed as field name "$feature" with format
    /// string "length".
    /// </summary>
    private const string GeometryLengthFieldName = "$feature.length";
    private const string GeometryAreaFieldName = "$feature.area";

    /// <summary>Extracts the field name from <c>[Field]</c> or a cast wrapping it, e.g. <c>float([Field])</c>/<c>str([Field])</c>.</summary>
    private static string? ExtractFieldFromNumericValue(string text)
    {
        var m = Regex.Match(text, @"^\[(?<field>[^\]]+)\]$");
        if (m.Success)
        {
            return m.Groups["field"].Value.Trim();
        }

        m = Regex.Match(text, $@"^(?:{NumericCastNames})\s*\(\s*\[(?<field>[^\]]+)\]\s*\)$", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            return m.Groups["field"].Value.Trim();
        }

        // "Length($feature)"/"$feature.Length" and "Area($feature)"/"$feature.Area" - the same
        // geometry accessors handled in the main tokenizer loop above, but reachable here too when
        // they appear directly inside round(...), e.g. "Round(Length($feature),1)".
        if (Regex.IsMatch(text, @"^length\s*\(\s*\$feature\s*\)$", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(text, @"^\$feature\.length$", RegexOptions.IgnoreCase))
        {
            return GeometryLengthFieldName;
        }
        if (Regex.IsMatch(text, @"^area\s*\(\s*\$feature\s*\)$", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(text, @"^\$feature\.area$", RegexOptions.IgnoreCase))
        {
            return GeometryAreaFieldName;
        }

        return null;
    }

    /// <summary>
    /// Parses a <c>(...)</c> right after the already-consumed <c>Length</c>/<c>Area</c> keyword
    /// (<paramref name="i"/> points just past it), requiring the parenthesized content to be
    /// exactly <c>$feature</c> (Arcade's geometry function-call form, e.g. <c>Length($feature)</c>)
    /// - anything else inside the parens is rejected rather than guessed at.
    /// </summary>
    private static bool TryParseFeatureGeometryCall(string expr, ref int i)
    {
        int n = expr.Length;
        int j = i;
        while (j < n && char.IsWhiteSpace(expr[j]))
        {
            j++;
        }
        if (j >= n || expr[j] != '(')
        {
            return false;
        }

        int closeParen = expr.IndexOf(')', j + 1);
        if (closeParen < 0)
        {
            return false; // unbalanced parens
        }

        var inner = expr[(j + 1)..closeParen].Trim();
        if (!inner.Equals("$feature", StringComparison.OrdinalIgnoreCase))
        {
            return false; // only a bare "$feature" argument is supported, not e.g. Length($feature.SubField) or Length([Field])
        }

        i = closeParen + 1;
        return true;
    }

    /// <summary>
    /// Parses a cast call like <c>str([Field])</c> standing alone in a concatenation (as opposed
    /// to wrapped inside <c>round(...)</c>) starting at the <c>(</c> right after the already-
    /// consumed cast keyword (<paramref name="i"/> points just past it). Since the cast is a
    /// no-op for our purposes, this just yields a plain <c>[Field]</c> reference.
    /// </summary>
    private static bool TryParsePassthroughCast(string expr, ref int i, out FieldTerm term)
    {
        term = null!;
        int n = expr.Length;
        int j = i;
        while (j < n && char.IsWhiteSpace(expr[j]))
        {
            j++;
        }
        if (j >= n || expr[j] != '(')
        {
            return false;
        }

        int depth = 0;
        int openParen = j;
        int closeParen = -1;
        for (int k = j; k < n; k++)
        {
            if (expr[k] == '(')
            {
                depth++;
            }
            else if (expr[k] == ')')
            {
                depth--;
                if (depth == 0)
                {
                    closeParen = k;
                    break;
                }
            }
        }
        if (closeParen < 0)
        {
            return false; // unbalanced parens
        }

        var inner = expr[(openParen + 1)..closeParen].Trim();
        var fieldName = ExtractFieldFromNumericValue(inner); // bare [Field], or one more nested cast
        if (fieldName == null)
        {
            return false;
        }

        term = new FieldTerm(fieldName);
        i = closeParen + 1;
        return true;
    }

    private static string RenderTerms(IEnumerable<Term> terms)
    {
        var sb = new StringBuilder();
        foreach (var term in terms)
        {
            switch (term)
            {
                case LiteralTerm lit:
                    sb.Append(lit.Text);
                    break;
                case FieldTerm f:
                    sb.Append('[').Append(f.Name);
                    if (f.Format != null)
                    {
                        sb.Append(':').Append(f.Format);
                    }
                    sb.Append(']');
                    break;
                case NewlineTerm:
                    sb.Append(Environment.NewLine);
                    break;
            }
        }
        return sb.ToString();
    }

    private static List<List<Term>> SplitIntoLines(List<Term> terms)
    {
        var lines = new List<List<Term>>();
        var current = new List<Term>();
        foreach (var t in terms)
        {
            if (t is NewlineTerm)
            {
                lines.Add(current);
                current = [];
            }
            else
            {
                current.Add(t);
            }
        }
        lines.Add(current);
        return lines;
    }
}
