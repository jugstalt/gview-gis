#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Data.Extensions;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Data.Sql;

/// <summary>
/// Validates where clauses that come from public service requests (GeoServices where/layerDefs/
/// definitionExpression, ArcXML, GeoJsonService) against an allowlist of SQL tokens.
/// <para>
/// Allowed: identifiers (fields, optionally "quoted" or [bracketed]), string and number literals,
/// NULL, TRUE/FALSE, date/time/timestamp literals, comparison operators, AND/OR/NOT/IN/LIKE/ESCAPE/IS/
/// BETWEEN, parentheses, comma, + - * / and calls of allowed functions
/// (<see cref="DefaultAllowedFunctions"/> plus <see cref="AdditionalAllowedFunctions"/>).
/// Everything else (SELECT, UNION, ;, comments, variables, EXEC, ...) is rejected.
/// </para>
/// <para>
/// The where clause is not rewritten: if every token is allowed, the original string is used.
/// Where clauses of the service configuration (layer filter queries) are trusted and not checked.
/// </para>
/// </summary>
public static class SqlWhereClauseValidator
{
    private const string Prefix = "select * from tab where ";
    private static readonly string[] PrefixTokenTypes = ["TOKEN_SELECT", "*", "TOKEN_FROM", "TOKEN_ID", "TOKEN_WHERE"];

    /// <summary>
    /// SQL-92 functions sent by common clients (Esri standardized queries).
    /// </summary>
    public static readonly IReadOnlyCollection<string> DefaultAllowedFunctions =
    [
        "UPPER", "LOWER", "CHAR_LENGTH", "TRIM", "CAST", "EXTRACT", "CURRENT_DATE", "CURRENT_TIMESTAMP"
    ];

    private static HashSet<string> _additionalAllowedFunctions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Functions allowed in addition to <see cref="DefaultAllowedFunctions"/>
    /// (gView.Server: mapserver.json security:sql:allowed-functions).
    /// </summary>
    public static IEnumerable<string> AdditionalAllowedFunctions
    {
        get => _additionalAllowedFunctions.ToArray();
        set
        {
            var functions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var function in value ?? [])
            {
                var name = function?.Trim() ?? "";
                if (name.Length == 0)
                {
                    continue;
                }
                if (!IsSimpleIdentifier(name))
                {
                    throw new ArgumentException($"Invalid function name '{name}' in allowed SQL functions");
                }
                functions.Add(name);
            }
            _additionalAllowedFunctions = functions;
        }
    }

    public static bool IsAllowedFunction(string name)
        => DefaultAllowedFunctions.Contains(name, StringComparer.OrdinalIgnoreCase)
        || _additionalAllowedFunctions.Contains(name);

    private static readonly HashSet<string> SimpleTokenTypes = new(StringComparer.Ordinal)
    {
        "TOKEN_INTEGER", "TOKEN_NUMERIC", "TOKEN_REAL",
        "TOKEN_NULL", "TOKEN_AND", "TOKEN_OR", "TOKEN_NOT", "TOKEN_IN", "TOKEN_LIKE", "TOKEN_ESCAPE", "TOKEN_IS", "TOKEN_BETWEEN",
        "=", "<", ">", "!", ",", "+", "-", "*", "/"
    };

    private static readonly HashSet<string> BooleanLiterals = new(StringComparer.OrdinalIgnoreCase) { "TRUE", "FALSE" };
    private static readonly HashSet<string> DateLiteralKeywords = new(StringComparer.OrdinalIgnoreCase) { "DATE", "TIME", "TIMESTAMP" };
    // words of multi word type names that the parser reads as keywords (e.g. DOUBLE PRECISION, CHARACTER VARYING, TIMESTAMP WITH TIME ZONE)
    private static readonly HashSet<string> CastTypeKeywords = new(StringComparer.OrdinalIgnoreCase) { "DOUBLE", "PRECISION", "CHARACTER", "CHAR", "NATIONAL", "VARYING", "WITH", "WITHOUT", "TIME", "ZONE", "DATE", "TIMESTAMP", "INT", "INTEGER", "FLOAT", "REAL", "DECIMAL", "NUMERIC", "VARCHAR", "NVARCHAR" };
    private static readonly HashSet<string> DateParts = new(StringComparer.OrdinalIgnoreCase) { "YEAR", "MONTH", "DAY", "HOUR", "MINUTE", "SECOND" };

    /// <summary>
    /// Throws <see cref="SqlDangerousStatementExceptions"/> if the where clause contains anything
    /// that is not on the allowlist. If <paramref name="tableClass"/> is set, every identifier
    /// must be a field of the table (case-insensitive).
    /// </summary>
    public static void Validate(string? where, ITableClass? tableClass = null)
    {
        if (String.IsNullOrWhiteSpace(where))
        {
            return;
        }

        var tokens = Tokenize(where);
        var parens = new Stack<string?>();  // function name for each open parenthesis (null = grouping)

        for (int i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];

            switch (token.Type)
            {
                case "(":
                    parens.Push(null);
                    break;
                case ")":
                    if (parens.Count == 0)
                    {
                        throw Rejected(token.Text, "unbalanced parenthesis");
                    }
                    parens.Pop();
                    break;
                case "TOKEN_STRING":
                    CheckStringLiteral(token.Text);
                    break;
                case "TOKEN_CURRENT_DATE":
                case "TOKEN_CURRENT_TIMESTAMP":
                    CheckFunction(token.Text);
                    break;
                case "TOKEN_AS":
                    // CAST(expr AS type[(n[,n])])
                    if (parens.Count == 0 || !"CAST".Equals(parens.Peek(), StringComparison.OrdinalIgnoreCase))
                    {
                        throw Rejected(token.Text);
                    }
                    i = SkipCastType(tokens, i + 1);
                    break;
                case "TOKEN_FROM":
                    // EXTRACT(part FROM expr): the date part is checked with the identifier
                    if (parens.Count == 0 || !"EXTRACT".Equals(parens.Peek(), StringComparison.OrdinalIgnoreCase)
                        || i == 0 || !DateParts.Contains(tokens[i - 1].Text))
                    {
                        throw Rejected(token.Text);
                    }
                    break;
                case "TOKEN_ID":
                    i = CheckIdentifier(tokens, i, parens, tableClass);
                    break;
                default:
                    if (!SimpleTokenTypes.Contains(token.Type))
                    {
                        throw Rejected(token.Text);
                    }
                    break;
            }
        }

        if (parens.Count > 0)
        {
            throw Rejected("(", "unbalanced parenthesis");
        }
    }

    #region Tokens

    private record SqlToken(string Type, string Text);

    private static SqlToken[] Tokenize(string where)
    {
        var parseResult = Parser.Parse(Prefix + where);
        var tokens = new List<SqlToken>();

        foreach (var token in parseResult.Script.Tokens)
        {
            if (token.Type == "LEX_WHITE")
            {
                // only plain whitespace: other unicode spaces may be read differently by the database
                if (token.Text.Any(c => c != ' ' && c != '\t' && c != '\r' && c != '\n'))
                {
                    throw Rejected("whitespace", "unsupported whitespace character");
                }
                continue;
            }
            tokens.Add(new SqlToken(token.Type, token.Text));
        }

        for (int i = 0; i < PrefixTokenTypes.Length; i++)
        {
            if (i >= tokens.Count || tokens[i].Type != PrefixTokenTypes[i])
            {
                throw Rejected(where, "can't be parsed");
            }
        }

        return tokens.Skip(PrefixTokenTypes.Length).ToArray();
    }

    private static void CheckStringLiteral(string text)
    {
        // 'text' or N'text', quotes inside doubled ('')
        var literal = text.StartsWith("N'", StringComparison.OrdinalIgnoreCase) ? text.Substring(1) : text;
        if (literal.Length < 2 || literal[0] != '\'' || literal[^1] != '\'')
        {
            throw Rejected(text, "unterminated string literal");
        }

        // databases with backslash escapes (e.g. PostgreSQL E'..' or standard_conforming_strings=off)
        // would end the literal somewhere else than this parser
        var inner = literal.Substring(1, literal.Length - 2);
        if (inner.Contains("\\'"))
        {
            throw Rejected(text, "backslash before quote in string literal");
        }
    }

    private static int CheckIdentifier(SqlToken[] tokens, int i, Stack<string?> parens, ITableClass? tableClass)
    {
        var token = tokens[i];
        var next = i + 1 < tokens.Length ? tokens[i + 1] : null;
        var previous = i > 0 ? tokens[i - 1] : null;

        // function call
        if (next?.Type == "(")
        {
            if (!IsSimpleIdentifier(token.Text) || previous?.Type == ".")
            {
                throw Rejected(token.Text, "function not allowed");
            }
            CheckFunction(token.Text);
            parens.Push(token.Text);
            return i + 1;
        }

        // DATE '2020-01-01', TIMESTAMP '...'
        if (next?.Type == "TOKEN_STRING")
        {
            if (!DateLiteralKeywords.Contains(token.Text))
            {
                throw Rejected(token.Text + next.Text);
            }
            CheckStringLiteral(next.Text);
            return i + 1;
        }

        if (BooleanLiterals.Contains(token.Text))
        {
            return i;
        }

        // EXTRACT(YEAR FROM ...)
        if (next?.Type == "TOKEN_FROM" && DateParts.Contains(token.Text)
            && parens.Count > 0 && "EXTRACT".Equals(parens.Peek(), StringComparison.OrdinalIgnoreCase))
        {
            return i;
        }

        // field name, optionally qualified: a.b.c
        var parts = new List<string> { UnquoteIdentifier(token.Text) };
        int last = i;
        while (last + 2 < tokens.Length && tokens[last + 1].Type == "." && tokens[last + 2].Type == "TOKEN_ID")
        {
            if (last + 3 < tokens.Length && tokens[last + 3].Type == "(")
            {
                throw Rejected(tokens[last + 2].Text, "function not allowed");
            }
            parts.Add(UnquoteIdentifier(tokens[last + 2].Text));
            last += 2;
        }
        if (last + 1 < tokens.Length && tokens[last + 1].Type == ".")
        {
            throw Rejected(".");
        }

        var fieldName = String.Join(".", parts);
        if (tableClass != null)
        {
            if (tableClass.FindFieldIgnoreCase(fieldName) == null)
            {
                throw new SqlDangerousStatementExceptions($"Unknown field '{fieldName}' in where clause");
            }
        }
        else if (parts.Count > 1 || parts[0].Length == 0)
        {
            // without a table only plain field names (no schema.table.field)
            throw Rejected(fieldName);
        }

        return last;
    }

    private static int SkipCastType(SqlToken[] tokens, int i)
    {
        // type: one or more words (DOUBLE PRECISION), optional (n) or (n,n)
        if (i >= tokens.Length || !((tokens[i].Type == "TOKEN_ID" && IsSimpleIdentifier(tokens[i].Text)) || CastTypeKeywords.Contains(tokens[i].Text)))
        {
            throw Rejected(i < tokens.Length ? tokens[i].Text : "AS", "invalid cast type");
        }
        while (i + 1 < tokens.Length
            && ((tokens[i + 1].Type == "TOKEN_ID" && IsSimpleIdentifier(tokens[i + 1].Text)) || CastTypeKeywords.Contains(tokens[i + 1].Text)))
        {
            i++;
        }
        if (i + 1 < tokens.Length && tokens[i + 1].Type == "(")
        {
            int j = i + 2;
            if (j < tokens.Length && tokens[j].Type == "TOKEN_INTEGER")
            {
                j++;
                if (j + 1 < tokens.Length && tokens[j].Type == "," && tokens[j + 1].Type == "TOKEN_INTEGER")
                {
                    j += 2;
                }
                if (j < tokens.Length && tokens[j].Type == ")")
                {
                    return j;
                }
            }
            throw Rejected(tokens[i].Text, "invalid cast type");
        }
        return i;
    }

    private static void CheckFunction(string name)
    {
        if (!IsAllowedFunction(name))
        {
            throw Rejected(name, "function not allowed");
        }
    }

    #endregion

    #region Identifiers

    internal static bool IsSimpleIdentifier(string name)
        => name.Length > 0
        && (Char.IsLetter(name[0]) || name[0] == '_')
        && name.All(c => Char.IsLetterOrDigit(c) || c == '_');

    private static string UnquoteIdentifier(string text)
    {
        if (text.Length >= 2 && text[0] == '"')
        {
            if (text[^1] != '"')
            {
                throw Rejected(text, "unterminated identifier");
            }
            return text.Substring(1, text.Length - 2).Replace("\"\"", "\"");
        }
        if (text.Length >= 2 && text[0] == '[')
        {
            if (text[^1] != ']')
            {
                throw Rejected(text, "unterminated identifier");
            }
            return text.Substring(1, text.Length - 2).Replace("]]", "]");
        }
        if (text.StartsWith("\"") || text.StartsWith("["))
        {
            throw Rejected(text, "unterminated identifier");
        }
        if (!IsSimpleIdentifier(text))
        {
            throw Rejected(text);
        }
        return text;
    }

    #endregion

    private static SqlDangerousStatementExceptions Rejected(string token, string reason = "token not allowed")
    {
        if (token.Length > 40)
        {
            token = token.Substring(0, 40) + "...";
        }
        return new SqlDangerousStatementExceptions($"Unsupported SQL in where clause: {reason} ('{token}')");
    }
}
