using System.Globalization;
using System.Text;

namespace gView.Interoperability.OgcApi.Cql2;

/// <summary>
/// Parses CQL2-Text (basic CQL2, advanced comparison operators LIKE/BETWEEN/IN,
/// case-insensitive comparison CASEI) into a <see cref="CqlPredicate"/> syntax tree.
/// Spatial, temporal and array operators are not supported.
/// </summary>
public sealed class CqlTextParser
{
    private readonly List<Token> _tokens;
    private int _position;

    private CqlTextParser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public static CqlPredicate Parse(string cql2Text)
    {
        if (String.IsNullOrWhiteSpace(cql2Text))
        {
            throw Error("empty filter");
        }

        var parser = new CqlTextParser(Tokenize(cql2Text));
        var predicate = parser.ParseOr();

        if (parser.Current.Type != TokenType.End)
        {
            throw Error($"unexpected '{parser.Current.Text}'");
        }

        return predicate;
    }

    #region Parser

    private Token Current => _tokens[_position];

    private Token Next() => _tokens[_position++];

    private bool IsKeyword(string keyword, int offset = 0)
    {
        var token = _tokens[Math.Min(_position + offset, _tokens.Count - 1)];
        return token.Type == TokenType.Identifier && token.Text.Equals(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private bool TryKeyword(string keyword)
    {
        if (IsKeyword(keyword))
        {
            _position++;
            return true;
        }
        return false;
    }

    private void Expect(TokenType type, string text)
    {
        if (Current.Type != type)
        {
            throw Error($"expected '{text}' but found '{Current.Text}'");
        }
        _position++;
    }

    private void ExpectKeyword(string keyword)
    {
        if (!TryKeyword(keyword))
        {
            throw Error($"expected '{keyword}' but found '{Current.Text}'");
        }
    }

    private CqlPredicate ParseOr()
    {
        var left = ParseAnd();
        while (TryKeyword("OR"))
        {
            left = new CqlOr(left, ParseAnd());
        }
        return left;
    }

    private CqlPredicate ParseAnd()
    {
        var left = ParseNot();
        while (TryKeyword("AND"))
        {
            left = new CqlAnd(left, ParseNot());
        }
        return left;
    }

    private CqlPredicate ParseNot()
    {
        if (TryKeyword("NOT"))
        {
            return new CqlNot(ParseNot());
        }
        return ParsePrimary();
    }

    private CqlPredicate ParsePrimary()
    {
        if (Current.Type == TokenType.LeftParen)
        {
            _position++;
            var predicate = ParseOr();
            Expect(TokenType.RightParen, ")");
            return predicate;
        }

        var left = ParseScalar();

        if (Current.Type == TokenType.Operator)
        {
            string op = Next().Text;
            return new CqlComparison(op, left, ParseScalar());
        }

        bool negated = false;
        if (IsKeyword("NOT") && (IsKeyword("LIKE", 1) || IsKeyword("BETWEEN", 1) || IsKeyword("IN", 1)))
        {
            _position++;
            negated = true;
        }

        if (TryKeyword("LIKE"))
        {
            return new CqlLike(left, ParseScalar(), negated);
        }

        if (TryKeyword("BETWEEN"))
        {
            var lower = ParseScalar();
            ExpectKeyword("AND");
            return new CqlBetween(left, lower, ParseScalar(), negated);
        }

        if (TryKeyword("IN"))
        {
            Expect(TokenType.LeftParen, "(");
            var list = new List<CqlExpression>() { ParseScalar() };
            while (Current.Type == TokenType.Comma)
            {
                _position++;
                list.Add(ParseScalar());
            }
            Expect(TokenType.RightParen, ")");
            return new CqlIn(left, list, negated);
        }

        if (TryKeyword("IS"))
        {
            bool isNot = TryKeyword("NOT");
            ExpectKeyword("NULL");
            return new CqlIsNull(left, isNot);
        }

        if (left is CqlBoolean booleanLiteral)
        {
            return new CqlBooleanPredicate(booleanLiteral.Value);
        }

        throw Error($"expected a comparison operator but found '{Current.Text}'");
    }

    private CqlExpression ParseScalar()
    {
        var token = Next();

        switch (token.Type)
        {
            case TokenType.String:
                return new CqlString(token.Text);

            case TokenType.Number:
                return new CqlNumber(double.Parse(token.Text, NumberStyles.Float, CultureInfo.InvariantCulture));

            case TokenType.QuotedIdentifier:
                return new CqlProperty(token.Text);

            case TokenType.Identifier:
                if (Current.Type == TokenType.LeftParen)
                {
                    return ParseFunction(token.Text);
                }

                switch (token.Text.ToUpperInvariant())
                {
                    case "TRUE":
                        return new CqlBoolean(true);
                    case "FALSE":
                        return new CqlBoolean(false);
                    case "AND":
                    case "OR":
                    case "NOT":
                    case "LIKE":
                    case "BETWEEN":
                    case "IN":
                    case "IS":
                    case "NULL":
                        throw Error($"unexpected keyword '{token.Text}'");
                }

                return new CqlProperty(token.Text);
        }

        throw Error(token.Type == TokenType.End
            ? "unexpected end of filter"
            : $"unexpected '{token.Text}'");
    }

    private CqlExpression ParseFunction(string name)
    {
        Expect(TokenType.LeftParen, "(");

        CqlExpression result;
        switch (name.ToUpperInvariant())
        {
            case "CASEI":
                result = new CqlCaseInsensitive(ParseScalar());
                break;

            case "DATE":
                result = new CqlDate(ParseDate(ExpectString()));
                break;

            case "TIMESTAMP":
                result = new CqlTimestamp(ParseTimestamp(ExpectString()));
                break;

            default:
                throw Error($"unsupported function '{name}'");
        }

        Expect(TokenType.RightParen, ")");
        return result;
    }

    private string ExpectString()
    {
        var token = Next();
        if (token.Type != TokenType.String)
        {
            throw Error($"expected a string literal but found '{token.Text}'");
        }
        return token.Text;
    }

    private static DateOnly ParseDate(string value)
    {
        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw Error($"invalid date '{value}' (yyyy-MM-dd expected)");
        }
        return date;
    }

    private static DateTime ParseTimestamp(string value)
    {
        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp)
            || !value.Contains('T', StringComparison.OrdinalIgnoreCase))
        {
            throw Error($"invalid timestamp '{value}' (RFC 3339 expected, e.g. 2024-05-17T10:30:00Z)");
        }
        return timestamp.UtcDateTime;
    }

    #endregion

    #region Lexer

    private enum TokenType
    {
        Identifier,
        QuotedIdentifier,
        String,
        Number,
        Operator,
        LeftParen,
        RightParen,
        Comma,
        End
    }

    private sealed record Token(TokenType Type, string Text);

    private static List<Token> Tokenize(string text)
    {
        var tokens = new List<Token>();
        int i = 0;

        while (i < text.Length)
        {
            char c = text[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
            }
            else if (c == '(')
            {
                tokens.Add(new Token(TokenType.LeftParen, "("));
                i++;
            }
            else if (c == ')')
            {
                tokens.Add(new Token(TokenType.RightParen, ")"));
                i++;
            }
            else if (c == ',')
            {
                tokens.Add(new Token(TokenType.Comma, ","));
                i++;
            }
            else if (c == '\'')
            {
                tokens.Add(new Token(TokenType.String, ReadQuoted(text, ref i, '\'')));
            }
            else if (c == '"')
            {
                tokens.Add(new Token(TokenType.QuotedIdentifier, ReadQuoted(text, ref i, '"')));
            }
            else if (c is '=' or '<' or '>')
            {
                string op = c.ToString();
                if (i + 1 < text.Length && (text[i + 1] == '=' || (c == '<' && text[i + 1] == '>')))
                {
                    op += text[i + 1];
                }
                tokens.Add(new Token(TokenType.Operator, op));
                i += op.Length;
            }
            else if (char.IsDigit(c)
                     || (c == '.' && i + 1 < text.Length && char.IsDigit(text[i + 1]))
                     || ((c == '-' || c == '+') && i + 1 < text.Length && (char.IsDigit(text[i + 1]) || text[i + 1] == '.')))
            {
                tokens.Add(new Token(TokenType.Number, ReadNumber(text, ref i)));
            }
            else if (char.IsLetter(c) || c == '_' || c == ':')
            {
                int start = i;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] is '_' or ':' or '.'))
                {
                    i++;
                }
                tokens.Add(new Token(TokenType.Identifier, text.Substring(start, i - start)));
            }
            else
            {
                throw Error($"unexpected character '{c}' at position {i + 1}");
            }
        }

        tokens.Add(new Token(TokenType.End, "end of filter"));
        return tokens;
    }

    private static string ReadQuoted(string text, ref int i, char quote)
    {
        var sb = new StringBuilder();
        i++;  // opening quote

        while (i < text.Length)
        {
            if (text[i] == quote)
            {
                if (i + 1 < text.Length && text[i + 1] == quote)  // escaped quote
                {
                    sb.Append(quote);
                    i += 2;
                    continue;
                }
                i++;  // closing quote
                return sb.ToString();
            }
            sb.Append(text[i++]);
        }

        throw Error($"missing closing {quote}");
    }

    private static string ReadNumber(string text, ref int i)
    {
        int start = i;
        if (text[i] is '-' or '+')
        {
            i++;
        }
        while (i < text.Length && (char.IsDigit(text[i]) || text[i] == '.'))
        {
            i++;
        }
        if (i < text.Length && (text[i] == 'e' || text[i] == 'E'))
        {
            i++;
            if (i < text.Length && (text[i] == '-' || text[i] == '+'))
            {
                i++;
            }
            while (i < text.Length && char.IsDigit(text[i]))
            {
                i++;
            }
        }

        string number = text.Substring(start, i - start);
        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !double.IsFinite(value))
        {
            throw Error($"invalid number '{number}'");
        }
        return number;
    }

    #endregion

    private static OgcApiException Error(string message)
        => OgcApiException.BadRequest($"Invalid CQL2 filter: {message}");
}
