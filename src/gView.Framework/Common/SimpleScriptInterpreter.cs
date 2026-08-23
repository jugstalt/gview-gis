using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace gView.Framework.Common
{
    /// <summary>
    /// Interprets gView's "@@start / @@if(...) / @@endif / @@end / @@replace(...)" mini-script,
    /// used for conditional/composite label expressions. This runs once per rendered label, so
    /// for a map service with many labels and many concurrent users it can run very often; the
    /// implementation below is written to avoid heap allocation on the hot path (no
    /// <see cref="string.Split(char[])"/>-style per-line/per-argument string arrays, no LINQ, no
    /// full-script line-ending normalization) by working on <see cref="ReadOnlySpan{T}"/> slices
    /// of the original <see cref="Script"/> string wherever possible. None of this changes
    /// observable behaviour - see gView.Framework.Tests.Common.SimpleScriptInterpreterTests.
    /// </summary>
    public class SimpleScriptInterpreter
    {
        // Generous upper bound for how many comma-separated arguments a single "@@if(...)"/
        // "@@replace(...)" can have. Comfortably covers every realistic case (including an "in"
        // list of category codes) many times over. If it's ever exceeded, ReadOnlySpan<char>.Split
        // folds the remaining, unparsed text into the last slot instead of throwing - so an
        // unrealistically long argument list degrades gracefully rather than crashing.
        private const int MaxArgs = 64;

        public SimpleScriptInterpreter(string script)
        {
            Script = script;
        }

        public string Script { get; set; }

        public string Interpret()
        {
            var script = Script;
            if (string.IsNullOrEmpty(script))
            {
                return script;
            }

            // EnumerateLines recognizes "\r\n", "\r" and "\n" uniformly, so there's no need to
            // normalize Environment.NewLine into "\n" first (which used to allocate a full copy of
            // the script up front) - and unlike the old Split('\n')-only approach, it also can't be
            // tripped up by a script authored with a different line ending than the current
            // platform's Environment.NewLine.
            var lines = script.AsSpan().EnumerateLines();

            // "@@start" must be its own first line, i.e. followed by an actual line break - a
            // value that merely starts with the literal text "@@start" (e.g. "@@startsWith") is
            // passed through as-is. If the input has no line break at all, EnumerateLines yields
            // the entire remaining text as this single first line, so it won't SequenceEqual
            // "@@start" unless that's genuinely the whole input - which correctly still counts as
            // "not followed by a newline" and is rejected by the MoveNext() below.
            if (!lines.MoveNext() || !lines.Current.SequenceEqual("@@start"))
            {
                return script;
            }

            var sb = new StringBuilder(script.Length);
            var result = script;
            bool interpret = false;

            // Stack of active "@@if(...)" conditions - a line is only emitted while every
            // enclosing condition is true, so "@@if(...)" blocks can be nested to express an
            // AND of several checks (e.g. "field A present" nested inside "field B present").
            // An empty stack means "no active condition" => line is shown (matches the
            // original, non-nested behaviour where a single @@if/@@endif pair was allowed).
            var conditionStack = new List<bool>();

            // Reused across iterations for "@@replace(search,replacement)" arguments (always
            // exactly 2) so the stackalloc isn't repeated on every loop pass.
            Span<Range> replaceArgRanges = stackalloc Range[3];

            while (lines.MoveNext())
            {
                var line = lines.Current;
                var command = GetCommand(line);

                if (line.SequenceEqual("@@end"))
                {
                    interpret = true;
                    result = sb.ToString();
                }
                else if (line.StartsWith("@@if("))
                {
                    conditionStack.Add(CheckCondition(command.ArgsText));
                }
                else if (line.SequenceEqual("@@endif"))
                {
                    if (conditionStack.Count > 0)
                    {
                        conditionStack.RemoveAt(conditionStack.Count - 1);
                    }
                }
                else if (!interpret && AllTrue(conditionStack))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(Environment.NewLine);
                    }

                    sb.Append(line);
                }

                if (interpret && command.Kind == CommandKind.Replace)
                {
                    int count = command.ArgsText.Split(replaceArgRanges, ',');
                    if (count == 2)
                    {
                        // string has no ReadOnlySpan<char> overload of Replace(search,replacement)
                        // - this materializes the two (typically short) arguments, but "@@replace"
                        // only ever appears a handful of times per script, unlike @@if(...), so
                        // it's not worth avoiding.
                        var search = command.ArgsText[replaceArgRanges[0]].ToString();
                        var replacement = command.ArgsText[replaceArgRanges[1]].ToString();
                        result = result.Replace(search, replacement);
                    }
                }
            }

            return result;
        }

        private static bool AllTrue(List<bool> conditionStack)
        {
            for (int i = 0; i < conditionStack.Count; i++)
            {
                if (!conditionStack[i])
                {
                    return false;
                }
            }
            return true;
        }

        private enum CommandKind { None, If, Replace, Other }

        /// <summary>
        /// A parsed "@@name(args)" line - a <see langword="ref struct"/> since it only ever holds
        /// spans into the caller's line, never materializing the command name or argument text as
        /// heap strings.
        /// </summary>
        private readonly ref struct ParsedCommand
        {
            public CommandKind Kind { get; init; }
            public ReadOnlySpan<char> ArgsText { get; init; }
        }

        private static ParsedCommand GetCommand(ReadOnlySpan<char> line)
        {
            line = line.Trim();

            if (!line.StartsWith("@@") || !line.EndsWith(")"))
            {
                return default;
            }

            var openParen = line.IndexOf('(');
            if (openParen < 0)
            {
                return default;
            }

            var commandName = line[2..openParen];
            var argsText = line[(openParen + 1)..^1];

            if (commandName.Equals("if", StringComparison.OrdinalIgnoreCase))
            {
                return new ParsedCommand { Kind = CommandKind.If, ArgsText = argsText };
            }
            if (commandName.Equals("replace", StringComparison.OrdinalIgnoreCase))
            {
                return new ParsedCommand { Kind = CommandKind.Replace, ArgsText = argsText };
            }

            return new ParsedCommand { Kind = CommandKind.Other, ArgsText = argsText };
        }

        private static bool CheckCondition(ReadOnlySpan<char> argsText)
        {
            Span<Range> ranges = stackalloc Range[MaxArgs];
            int count = argsText.Split(ranges, ',');

            // The "value,op,threshold" 3-argument forms are always generated with a comma-free
            // operator keyword and a comma-free (invariant-formatted) threshold - only the first
            // argument ever comes from substituted field data, and under some locales a numeric
            // field's own ToString() uses "," as the decimal separator (e.g. "125,4"), which
            // fragments naive comma-splitting into more than 3 pieces. Recombine: if there are
            // more than 3 pieces and the second-to-last one is a recognized operator, everything
            // before it must actually be a single, comma-fragmented first argument.
            if (count > 3)
            {
                var maybeOp = argsText[ranges[count - 2]];
                if (IsScalarOperator(maybeOp))
                {
                    var combinedValue = argsText[new Range(ranges[0].Start, ranges[count - 3].End)];
                    return EvaluateScalarOperator(combinedValue, maybeOp, argsText[ranges[count - 1]]);
                }
            }

            if (count == 1)
            {
                return !argsText[ranges[0]].IsWhiteSpace();
            }
            if (count == 2)
            {
                return argsText[ranges[0]].SequenceEqual(argsText[ranges[1]]);
            }
            if (count >= 3 && argsText[ranges[1]].Equals("in", StringComparison.OrdinalIgnoreCase))
            {
                // "value,in,v1,v2,...": true if the value equals any of v1..vN (one or more
                // values). Lets a condition on one field with several acceptable values collapse
                // into a single check instead of several near-duplicate "@@if(...)" blocks.
                var value = argsText[ranges[0]];
                for (int k = 2; k < count; k++)
                {
                    if (value.SequenceEqual(argsText[ranges[k]]))
                    {
                        return true;
                    }
                }
                return false;
            }
            if (count == 3)
            {
                return EvaluateScalarOperator(argsText[ranges[0]], argsText[ranges[1]], argsText[ranges[2]]);
            }

            return false;
        }

        private static bool IsScalarOperator(ReadOnlySpan<char> op) =>
            op.Equals("eq", StringComparison.OrdinalIgnoreCase) ||
            op.Equals("not", StringComparison.OrdinalIgnoreCase) ||
            op.Equals("lt", StringComparison.OrdinalIgnoreCase) ||
            op.Equals("le", StringComparison.OrdinalIgnoreCase) ||
            op.Equals("gt", StringComparison.OrdinalIgnoreCase) ||
            op.Equals("ge", StringComparison.OrdinalIgnoreCase);

        private static bool EvaluateScalarOperator(ReadOnlySpan<char> a, ReadOnlySpan<char> op, ReadOnlySpan<char> b)
        {
            if (op.Equals("eq", StringComparison.OrdinalIgnoreCase))
            {
                return a.SequenceEqual(b);
            }
            if (op.Equals("not", StringComparison.OrdinalIgnoreCase))
            {
                return !a.SequenceEqual(b);
            }
            if (op.Equals("lt", StringComparison.OrdinalIgnoreCase))
            {
                return TryCompareNumeric(a, b, out var c1) && c1 < 0;
            }
            if (op.Equals("le", StringComparison.OrdinalIgnoreCase))
            {
                return TryCompareNumeric(a, b, out var c2) && c2 <= 0;
            }
            if (op.Equals("gt", StringComparison.OrdinalIgnoreCase))
            {
                return TryCompareNumeric(a, b, out var c3) && c3 > 0;
            }
            if (op.Equals("ge", StringComparison.OrdinalIgnoreCase))
            {
                return TryCompareNumeric(a, b, out var c4) && c4 >= 0;
            }

            return false; // unrecognized operator -> condition is false
        }

        /// <summary>
        /// Parses both values as numbers - trying the invariant culture first, then the current
        /// culture (a plain "[Field]" placeholder is substituted via the field value's own
        /// <c>ToString()</c>, which for a number uses the current culture, e.g. "123,4" rather
        /// than "123.4" under a German locale) - and compares them. Returns <see langword="false"/>
        /// if either side isn't a number.
        /// </summary>
        private static bool TryCompareNumeric(ReadOnlySpan<char> a, ReadOnlySpan<char> b, out int comparison)
        {
            comparison = 0;
            if (!TryParseNumber(a, out var da) || !TryParseNumber(b, out var db))
            {
                return false;
            }
            comparison = da.CompareTo(db);
            return true;
        }

        private static bool TryParseNumber(ReadOnlySpan<char> s, out double value) =>
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value);

        public static bool IsSimpleScript(string script)
        {
            return script.StartsWith("@@start");
        }
    }
}
