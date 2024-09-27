namespace ExpressionFiltering.ExpressionParser;

public enum TokenType
{
    Identifier,
    StringLiteral,
    NumberLiteral,
    Operator,
    OpenParen,
    CloseParen,
    EOF
}

public class Token
{
    public TokenType Type { get; }
    public string Text { get; }
    public Token(TokenType type, string text)
    {
        Type = type;
        Text = text;
    }
}
