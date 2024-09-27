namespace ExpressionFiltering.ExpressionParser;

public class Tokenizer
{
    private readonly string _text;
    private int _position;

    public Tokenizer(string text)
    {
        _text = text;
    }

    public IEnumerable<Token> Tokenize()
    {
        while (_position < _text.Length)
        {
            char current = _text[_position];

            if (char.IsWhiteSpace(current))
            {
                _position++;
            }
            else if (current == '(')
            {
                yield return new Token(TokenType.OpenParen, current.ToString());
                _position++;
            }
            else if (current == ')')
            {
                yield return new Token(TokenType.CloseParen, current.ToString());
                _position++;
            }
            else if (IsOperatorStart(current))
            {
                yield return ReadOperator();
            }
            else if (char.IsLetter(current) || current == '_')
            {
                yield return ReadIdentifier();
            }
            else if (current == '\'' || current == '\"')
            {
                yield return ReadStringLiteral();
            }
            else if (char.IsDigit(current))
            {
                yield return ReadNumberLiteral();
            }
            else
            {
                throw new Exception($"Unbekanntes Zeichen '{current}' an Position {_position}.");
            }
        }

        yield return new Token(TokenType.EOF, string.Empty);
    }

    private Token ReadOperator()
    {
        int start = _position;
        while (_position < _text.Length && IsOperatorPart(_text[_position]))
        {
            _position++;
        }
        string op = _text.Substring(start, _position - start);
        return new Token(TokenType.Operator, op);
    }

    private Token ReadIdentifier()
    {
        int start = _position;
        while (_position < _text.Length && (char.IsLetterOrDigit(_text[_position]) || _text[_position] == '_'))
        {
            _position++;
        }
        string identifier = _text.Substring(start, _position - start);
        return new Token(TokenType.Identifier, identifier);
    }

    private Token ReadStringLiteral()
    {
        char quote = _text[_position];
        _position++; // Überspringe das öffnende Anführungszeichen
        int start = _position;

        while (_position < _text.Length && _text[_position] != quote)
        {
            _position++;
        }

        if (_position >= _text.Length)
            throw new Exception("Unbeendeter String-Literal.");

        string value = _text.Substring(start, _position - start);
        _position++; // Überspringe das schließende Anführungszeichen

        return new Token(TokenType.StringLiteral, value);
    }

    private Token ReadNumberLiteral()
    {
        int start = _position;
        while (_position < _text.Length && char.IsDigit(_text[_position]))
        {
            _position++;
        }

        if (_position < _text.Length && _text[_position] == '.')
        {
            _position++;
            while (_position < _text.Length && char.IsDigit(_text[_position]))
            {
                _position++;
            }
        }

        string number = _text.Substring(start, _position - start);
        return new Token(TokenType.NumberLiteral, number);
    }

    private bool IsOperatorStart(char c)
    {
        return "=!<>|&".Contains(c);
    }

    private bool IsOperatorPart(char c)
    {
        return "=!<>|&".Contains(c);
    }
}
