using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionFiltering.ExpressionParser;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _position;
    private ParameterExpression _parameter;

    public Parser(IEnumerable<Token> tokens)
    {
        _tokens = tokens.Where(t => t.Type != TokenType.EOF).ToList();
        _parameter = Expression.Parameter(typeof(IDictionary<string, object>), "dict");
    }

    public Expression<Func<IDictionary<string, object>, bool>> ParseExpression()
    {
        Expression expr = ParseOrExpression();
        return Expression.Lambda<Func<IDictionary<string, object>, bool>>(expr, _parameter);
    }

    private Expression ParseOrExpression()
    {
        Expression left = ParseAndExpression();

        while (Match(TokenType.Operator, "||"))
        {
            var operatorToken = Previous();
            Expression right = ParseAndExpression();
            left = Expression.OrElse(left, right);
        }

        return left;
    }

    private Expression ParseAndExpression()
    {
        Expression left = ParseEqualityExpression();

        while (Match(TokenType.Operator, "&&"))
        {
            var operatorToken = Previous();
            Expression right = ParseEqualityExpression();
            left = Expression.AndAlso(left, right);
        }

        return left;
    }

    private Expression ParseEqualityExpression()
    {
        Expression left = ParseRelationalExpression();

        while (Match(TokenType.Operator, "==", "!="))
        {
            var operatorToken = Previous();
            Expression right = ParseRelationalExpression();
            left = BuildComparisonExpression(operatorToken.Text, left, right);
        }

        return left;
    }

    private Expression ParseRelationalExpression()
    {
        Expression left = ParsePrimary();

        while (Match(TokenType.Operator, ">", "<", ">=", "<="))
        {
            var operatorToken = Previous();
            Expression right = ParsePrimary();
            left = BuildComparisonExpression(operatorToken.Text, left, right);
        }

        return left;
    }

    private Expression ParsePrimary()
    {
        if (Match(TokenType.Identifier))
        {
            return GetValueExpression(_parameter, Previous().Text);
        }

        if (Match(TokenType.StringLiteral))
        {
            string value = Previous().Text;
            return Expression.Constant(value);
        }

        if (Match(TokenType.NumberLiteral))
        {
            string text = Previous().Text;
            if (text.Contains("."))
            {
                double value = double.Parse(text, CultureInfo.InvariantCulture);
                return Expression.Constant(value);
            }
            else
            {
                int value = int.Parse(text, CultureInfo.InvariantCulture);
                return Expression.Constant(value);
            }
        }

        if (Match(TokenType.OpenParen))
        {
            Expression expr = ParseOrExpression();
            Consume(TokenType.CloseParen, "Erwartet ')'.");
            return expr;
        }

        throw new Exception("Unbekannter Ausdruck.");
    }

    private Expression BuildComparisonExpression(string op, Expression left, Expression right)
    {
        // Beide Operanden in IComparable konvertieren
        left = Expression.Convert(left, typeof(IComparable));
        right = Expression.Convert(right, typeof(object));

        // Expression für left.CompareTo(right)
        MethodInfo compareToMethod = typeof(IComparable).GetMethod("CompareTo", new Type[] { typeof(object) })!;
        Expression compareToCall = Expression.Call(left, compareToMethod, right);

        // Konstante '0' zum Vergleich
        Expression zero = Expression.Constant(0, typeof(int));

        switch (op)
        {
            case "==":
                return Expression.Equal(compareToCall, zero);
            case "!=":
                return Expression.NotEqual(compareToCall, zero);
            case ">":
                return Expression.GreaterThan(compareToCall, zero);
            case "<":
                return Expression.LessThan(compareToCall, zero);
            case ">=":
                return Expression.GreaterThanOrEqual(compareToCall, zero);
            case "<=":
                return Expression.LessThanOrEqual(compareToCall, zero);
            default:
                throw new NotSupportedException($"Operator '{op}' wird nicht unterstützt.");
        }
    }

    private Expression GetValueExpression(ParameterExpression param, string key)
    {
        // Ausdruck für dict[key]
        MethodCallExpression getItem = Expression.Call(
            param,
            typeof(IDictionary<string, object>).GetMethod("get_Item")!,
            Expression.Constant(key));

        // Konvertierung zu Object
        return Expression.Convert(getItem, typeof(object));
    }

    // Hilfsfunktionen für den Parser

    private bool Match(TokenType type, params string[] texts)
    {
        if (Check(type, texts))
        {
            _position++;
            return true;
        }
        return false;
    }

    private bool Check(TokenType type, params string[] texts)
    {
        if (IsAtEnd()) return false;
        var token = Peek();
        if (token.Type != type) return false;
        if (texts.Length > 0 && !texts.Contains(token.Text)) return false;
        return true;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }
        throw new Exception(message);
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _position++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return _position >= _tokens.Count;
    }

    private Token Peek()
    {
        return _tokens[_position];
    }

    private Token Previous()
    {
        return _tokens[_position - 1];
    }
}
