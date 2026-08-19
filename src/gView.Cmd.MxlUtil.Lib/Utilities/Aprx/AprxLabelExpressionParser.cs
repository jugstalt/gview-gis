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
/// This only handles a deliberately small subset of VBScript:
/// <list type="bullet">
/// <item>a plain string literal, e.g. <c>"Leerrohr vorh."</c> - any ArcGIS Pro rich-text
/// formatting tags inside it (<c>&lt;CLR .../&gt;</c>, <c>&lt;BOL&gt;</c>, ...) are stripped,
/// since gView's label text symbols can't render per-run formatting - see <see cref="RichTextTag"/>.</item>
/// <item>a concatenation (via <c>&amp;</c> or <c>+</c>) of string literals, <c>[Field]</c>
/// references, <c>round(...)</c> calls and <c>vbNewLine</c>/<c>vbCrLf</c>/<c>vbTab</c>,
/// e.g. <c>"lt. SAP: " &amp; [LABEL_SAP]</c> or <c>[SCHIEBERTYP] +[STATNR]</c></item>
/// <item>a <c>Function ... End Function</c> wrapper around a single such assignment</item>
/// <item>a <c>Function ... End Function</c> wrapper with an <c>If/ElseIf/[Else]/End If</c> chain,
/// where every branch condition is a set of <c>[Field] &lt;&gt; ""</c> / <c>[Field] = ""</c> /
/// <c>[Field] = "Value"</c> / <c>[Field] &lt;&gt; "Value"</c> checks joined with <c>and</c>,
/// optionally including one or more parenthesized <c>or</c> groups (e.g.
/// <c>[A] = "N" and ([B] = "HL" or [B] = "SL")</c>) - see <see cref="ParseConditionExpanded"/>.
/// Each branch's own text is reproduced verbatim behind a guard that reconstructs VB's
/// first-match semantics - see <see cref="TryBuildConditionalScript"/>.</item>
/// </list>
/// Anything else (function calls, arithmetic, <c>or</c> mixed with <c>and</c> without parentheses
/// to disambiguate, nested ifs, ...) is rejected so the caller can fall back to keeping the
/// original expression with a warning. Every conditional reduction is exhaustively verified
/// against every possible combination of field values worth distinguishing before being
/// accepted, so a rejected (unsupported) shape never produces wrong output - it only ever falls
/// back to the warning.
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
    private enum PredicateKind { NonEmpty, Empty, Equals, NotEquals }

    /// <summary>
    /// One VB condition atom on a single field, e.g. <c>[FIELD] &lt;&gt; ""</c> or
    /// <c>[FIELD] = "Schieber"</c>. Mirrors exactly what gView's
    /// <see cref="gView.Framework.Common.SimpleScriptInterpreter"/> checks for the matching
    /// <c>@@if(...)</c> form, so <see cref="Evaluate"/> can be used to predict its real behaviour.
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
            _ => false
        };

        public Predicate Negate() => Kind switch
        {
            PredicateKind.NonEmpty => this with { Kind = PredicateKind.Empty },
            PredicateKind.Empty => this with { Kind = PredicateKind.NonEmpty },
            PredicateKind.Equals => this with { Kind = PredicateKind.NotEquals },
            PredicateKind.NotEquals => this with { Kind = PredicateKind.Equals },
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
            // 3-arg "not equal" against a specific literal.
            PredicateKind.NotEquals => $"[{Field}],not,{Value}",
            _ => throw new InvalidOperationException($"Unhandled {nameof(PredicateKind)}: {Kind}")
        };
    }

    /// <summary>
    /// Parses a VB condition such as <c>[A] &lt;&gt; "" and [B] = "Schieber"</c> - optionally with
    /// parenthesized <c>or</c> groups AND-ed in, e.g.
    /// <c>[SA1] = "N" and ([SA2] = "HL" or [SA2] = "SL" or [SA2] = "VL")</c> - into the list of
    /// AND-only predicate combinations it's equivalent to (distributing "and" over "or":
    /// <c>A and (B or C)</c> becomes the two combinations <c>[A,B]</c> and <c>[A,C]</c>, one per
    /// branch that will later get its own guard). A bare, unparenthesized top-level <c>or</c>
    /// chain (no <c>and</c> at all) is also accepted the same way. Returns <see langword="null"/>
    /// for anything else (numeric comparisons, function calls, <c>or</c> mixed with <c>and</c>
    /// without parentheses to disambiguate, ...).
    /// </summary>
    private static List<List<Predicate>>? ParseConditionExpanded(string cond)
    {
        var andParts = SplitTopLevel(cond, "and");

        List<List<Predicate>> combinations = [[]];

        foreach (var rawPart in andParts)
        {
            var part = rawPart.Trim();

            List<Predicate>? alternatives;
            if (IsFullyParenthesized(part))
            {
                alternatives = ParseOrGroup(part[1..^1]);
            }
            else if (andParts.Count == 1)
            {
                // The whole condition may itself be a bare "or" chain with no enclosing parens.
                alternatives = ParseOrGroup(part);
            }
            else if (Regex.IsMatch(part, @"\bor\b", RegexOptions.IgnoreCase))
            {
                return null; // "or" mixed with "and" but not parenthesized - ambiguous, reject
            }
            else
            {
                var atom = ParseConditionAtom(part);
                alternatives = atom != null ? [atom] : null;
            }

            if (alternatives == null || alternatives.Count == 0)
            {
                return null;
            }

            var next = new List<List<Predicate>>();
            foreach (var combo in combinations)
            {
                foreach (var alt in alternatives)
                {
                    next.Add([.. combo, alt]);
                }
            }
            combinations = next;

            // Guard against pathological blow-up from multiplying several "or" groups together.
            if (combinations.Count > 64)
            {
                return null;
            }
        }

        return combinations;
    }

    /// <summary>Parses an <c>atom (or atom)*</c> chain (no nested "and") into its alternatives.</summary>
    private static List<Predicate>? ParseOrGroup(string text)
    {
        var orAtoms = SplitTopLevel(text, "or");

        var alternatives = new List<Predicate>();
        foreach (var raw in orAtoms)
        {
            var atomText = raw.Trim();
            if (Regex.IsMatch(atomText, @"\band\b", RegexOptions.IgnoreCase))
            {
                return null; // nested "and" inside an "or" group isn't supported
            }

            var atom = ParseConditionAtom(atomText);
            if (atom == null)
            {
                return null;
            }

            alternatives.Add(atom);
        }

        return alternatives.Count > 0 ? alternatives : null;
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
    /// Parses one condition atom: <c>[FIELD] (&lt;&gt;|=) "Value"</c>, in either operand order.
    /// An empty literal ("") maps to a plain non-empty/empty check; a non-empty literal maps to
    /// an equals/not-equals check.
    /// </summary>
    private static Predicate? ParseConditionAtom(string atom)
    {
        const string valuePattern = @"""(?<value>(?:[^""]|"""")*)""";

        var m = Regex.Match(atom, $@"^\[(?<field>[^\]]+)\]\s*(?<op><>|=)\s*{valuePattern}$");
        if (!m.Success)
        {
            m = Regex.Match(atom, $@"^{valuePattern}\s*(?<op><>|=)\s*\[(?<field>[^\]]+)\]$");
        }
        if (!m.Success)
        {
            return null;
        }

        var field = m.Groups["field"].Value.Trim();
        var op = m.Groups["op"].Value;
        var value = m.Groups["value"].Value.Replace("\"\"", "\""); // VB escapes a literal quote as ""

        if (value.Length == 0)
        {
            return new Predicate(field, op == "<>" ? PredicateKind.NonEmpty : PredicateKind.Empty);
        }

        return new Predicate(field, op == "<>" ? PredicateKind.NotEquals : PredicateKind.Equals, value);
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
    private static bool TryBuildConditionalScript(List<Branch> branches, out ConversionResult? result)
    {
        result = null;

        // For each field referenced anywhere, the values worth exercising: "" (empty), every
        // literal it's compared against, and one synthetic "some other value" to cover "present
        // but not equal to anything specifically tested" - added once domains are collected.
        var fieldDomains = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var predicate in branches.SelectMany(b => b.Conditions))
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

        var fields = fieldDomains.Keys.ToList();

        // Guard against pathological blow-up; real-world label expressions compare a handful
        // of fields against a handful of values at most.
        if (fields.Count > 8)
        {
            return false;
        }

        foreach (var field in fields)
        {
            var domain = fieldDomains[field];
            var other = "OTHER";
            for (int n = 0; domain.Contains(other, StringComparer.Ordinal); n++)
            {
                other = $"OTHER{n}";
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

        // For each branch, the predicates required by any *earlier* branch but not (exactly) by
        // this one must be negated - otherwise that earlier (more specific) branch would have
        // matched first in the original VB If/ElseIf chain.
        var excludePerBranch = new List<List<Predicate>>();
        var earlierConditions = new HashSet<Predicate>();
        foreach (var branch in branches)
        {
            var ownConditions = new HashSet<Predicate>(branch.Conditions);
            var exclude = earlierConditions
                .Where(p => !ownConditions.Contains(p))
                .Select(p => p.Negate())
                .Distinct()
                .ToList();
            excludePerBranch.Add(exclude);

            foreach (var p in branch.Conditions)
            {
                earlierConditions.Add(p);
            }
        }

        if (!TryVerifyBranchGuards(branches, excludePerBranch, fields, fieldDomains))
        {
            return false;
        }

        result = new ConversionResult(RenderConditionalScript(branches, excludePerBranch), IsConditional: true);
        return true;
    }

    /// <summary>
    /// Exhaustively verifies, for every combination of field values across the domains collected
    /// in <see cref="TryBuildConditionalScript"/>, that the per-branch guards (positive =
    /// <paramref name="branches"/>' own conditions, negative = <paramref name="excludePerBranch"/>)
    /// reproduce exactly the same text as the original VB If/ElseIf/[Else] chain - including the
    /// "no branch matches" case, where VB implicitly returns an empty string.
    /// </summary>
    private static bool TryVerifyBranchGuards(
        List<Branch> branches,
        List<List<Predicate>> excludePerBranch,
        List<string> fields,
        Dictionary<string, List<string>> fieldDomains)
    {
        var current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        return VerifyAllCombinations(branches, excludePerBranch, fields, fieldDomains, 0, current);
    }

    private static bool VerifyAllCombinations(
        List<Branch> branches,
        List<List<Predicate>> excludePerBranch,
        List<string> fields,
        Dictionary<string, List<string>> fieldDomains,
        int fieldIndex,
        Dictionary<string, string> values)
    {
        if (fieldIndex == fields.Count)
        {
            return VerifyOneCombination(branches, excludePerBranch, values);
        }

        var field = fields[fieldIndex];
        foreach (var value in fieldDomains[field])
        {
            values[field] = value;
            if (!VerifyAllCombinations(branches, excludePerBranch, fields, fieldDomains, fieldIndex + 1, values))
            {
                return false;
            }
        }
        values.Remove(field);
        return true;
    }

    private static bool VerifyOneCombination(
        List<Branch> branches,
        List<List<Predicate>> excludePerBranch,
        Dictionary<string, string> values)
    {
        string ValueOf(string field) => values.TryGetValue(field, out var v) ? v : "";
        bool AllTrue(IEnumerable<Predicate> predicates) => predicates.All(p => p.Evaluate(ValueOf(p.Field)));

        // What the original VB If/ElseIf/[Else] chain would produce (first branch whose
        // conditions are all satisfied; no match => empty, matching VB's default).
        var vbTerms = branches.FirstOrDefault(b => AllTrue(b.Conditions))?.Terms ?? [];
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
    /// Tokenizes a VB concatenation expression (string literals, <c>[Field]</c> references,
    /// <c>vbNewLine</c>/<c>vbCrLf</c>/<c>vbLf</c>/<c>vbCr</c>/<c>vbTab</c> constants,
    /// <c>round(...)</c> calls (see <see cref="TryParseRoundCall"/>), joined with <c>&amp;</c>)
    /// into a term list. Returns <see langword="false"/> for anything else (other function
    /// calls, other operators, bare numbers, ...).
    /// </summary>
    private static bool TryTokenizeConcatExpression(string expr, out List<Term> terms)
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

            if (c == '"')
            {
                i++;
                while (i < n)
                {
                    if (expr[i] == '"')
                    {
                        if (i + 1 < n && expr[i + 1] == '"') // "" => escaped quote inside literal
                        {
                            pendingLiteral.Append('"');
                            i += 2;
                            continue;
                        }
                        i++; // closing quote
                        break;
                    }
                    pendingLiteral.Append(expr[i]);
                    i++;
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

            if (c == '&' || c == '+')
            {
                // Concatenation operator ("+" is VBScript's other string-concatenation operator,
                // alongside "&"; we never accept numeric literals, so within the subset this
                // tokenizer understands it can't mean numeric addition) - just a separator
                // between terms, dropped like "&".
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

    /// <summary>Extracts the field name from <c>[Field]</c> or a numeric cast wrapping it, e.g. <c>float([Field])</c>.</summary>
    private static string? ExtractFieldFromNumericValue(string text)
    {
        var m = Regex.Match(text, @"^\[(?<field>[^\]]+)\]$");
        if (m.Success)
        {
            return m.Groups["field"].Value.Trim();
        }

        m = Regex.Match(text, @"^(float|cdbl|cdec|csng|val|cint|clng)\s*\(\s*\[(?<field>[^\]]+)\]\s*\)$", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups["field"].Value.Trim() : null;
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
