namespace ExpressionFiltering.ExpressionParser;

public class SafeFilterEvaluator
{
    private static readonly HashSet<string> AllowedOperators = new HashSet<string>
    {
        "==", "!=", ">", "<", ">=", "<=", "&&", "||"
    };

    private static readonly HashSet<string> AllowedFields = new HashSet<string>
    {
        "NAME", "FIELD1", "FIELD2"
        // Fügen Sie weitere erlaubte Feldnamen hinzu
    };

    public static Func<IDictionary<string, object>, bool> BuildSafePredicate(string filterCondition)
    {
        var tokenizer = new Tokenizer(filterCondition);
        var tokens = tokenizer.Tokenize().ToArray();

        // Sicherheitsüberprüfungen der Tokens
        foreach (var token in tokens)
        {
            if (token.Type == TokenType.Operator && !AllowedOperators.Contains(token.Text))
            {
                throw new Exception($"Forbidden operator '{token.Text}' in condition.");
            }

            if (token.Type == TokenType.Identifier && !AllowedFields.Contains(token.Text))
            {
                throw new Exception($"Forbidden fieldname '{token.Text}' in condition.");
            }
        }

        var parser = new Parser(tokens);
        var lambda = parser.ParseExpression();;

        return lambda.Compile();
    }
}
