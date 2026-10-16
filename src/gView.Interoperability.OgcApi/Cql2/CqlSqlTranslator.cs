using gView.Framework.Core.Data;
using gView.Interoperability.OgcApi.Extensions;
using System.Globalization;

namespace gView.Interoperability.OgcApi.Cql2;

/// <summary>
/// Translates a CQL2 syntax tree into a SQL where clause for one data source.
/// Property names are resolved against the queryables (unknown names => 400), literals are
/// written by the translator itself (escaped strings, invariant numbers, dialect specific
/// date/boolean literals), so the result can't contain injected SQL.
/// </summary>
public sealed class CqlSqlTranslator
{
    private enum ValueType
    {
        Text,
        Number,
        Boolean,
        Temporal,
        Any
    }

    private sealed record TypedOperand(CqlExpression Expression, ValueType Type);

    private readonly Func<string, IField?> _resolveField;
    private readonly SqlDialect _dialect;

    public CqlSqlTranslator(Func<string, IField?> resolveField, SqlDialect dialect)
    {
        _resolveField = resolveField;
        _dialect = dialect;
    }

    public string Translate(CqlPredicate predicate)
        => predicate switch
        {
            CqlAnd and => $"({Translate(and.Left)} AND {Translate(and.Right)})",
            CqlOr or => $"({Translate(or.Left)} OR {Translate(or.Right)})",
            CqlNot not => $"NOT ({Translate(not.Predicate)})",
            CqlBooleanPredicate b => b.Value ? "1=1" : "1=0",
            CqlComparison comparison => TranslateComparison(comparison),
            CqlLike like => TranslateLike(like),
            CqlBetween between => TranslateBetween(between),
            CqlIn @in => TranslateIn(@in),
            CqlIsNull isNull => $"{RenderPropertyOnly(isNull.Value)} IS {(isNull.Negated ? "NOT " : "")}NULL",
            _ => throw Error("unsupported predicate")
        };

    #region Predicates

    private string TranslateComparison(CqlComparison comparison)
    {
        if (comparison.Operator is not ("=" or "<>" or "<" or ">" or "<=" or ">="))
        {
            throw Error($"unsupported operator '{comparison.Operator}'");
        }

        var operands = Operands(comparison.Left, comparison.Right);
        return $"{Render(operands[0], operands)} {comparison.Operator} {Render(operands[1], operands)}";
    }

    private string TranslateLike(CqlLike like)
    {
        var value = Operand(like.Value);
        var pattern = Operand(like.Pattern);

        if (value.Type is not (ValueType.Text or ValueType.Any) || pattern.Type is not (ValueType.Text or ValueType.Any))
        {
            throw Error("LIKE can only be used with text");
        }
        if (Unwrap(like.Pattern) is not CqlString patternLiteral)
        {
            throw Error("the LIKE pattern must be a string literal");
        }

        string sql = $"{Render(value, [value])} {(like.Negated ? "NOT " : "")}LIKE {Render(pattern, [pattern])}";

        // CQL2 uses '\' to escape the wildcards % and _
        return patternLiteral.Value.Contains('\\')
            ? $"{sql} ESCAPE '\\'"
            : sql;
    }

    private string TranslateBetween(CqlBetween between)
    {
        var operands = Operands(between.Value, between.Lower, between.Upper);
        if (operands.Any(o => o.Type is ValueType.Text or ValueType.Boolean))
        {
            throw Error("BETWEEN can only be used with numbers or dates");
        }

        return $"{Render(operands[0], operands)} {(between.Negated ? "NOT " : "")}BETWEEN {Render(operands[1], operands)} AND {Render(operands[2], operands)}";
    }

    private string TranslateIn(CqlIn @in)
    {
        var operands = Operands([@in.Value, .. @in.List]);

        return $"{Render(operands[0], operands)} {(@in.Negated ? "NOT " : "")}IN ({String.Join(", ", operands.Skip(1).Select(o => Render(o, operands)))})";
    }

    #endregion

    #region Operands

    /// <summary>
    /// Resolves the operands and checks that all of them have the same type
    /// </summary>
    private TypedOperand[] Operands(params CqlExpression[] expressions)
    {
        var operands = expressions.Select(Operand).ToArray();

        var types = operands.Select(o => o.Type).Where(t => t != ValueType.Any).Distinct().ToArray();
        if (types.Length > 1)
        {
            throw Error($"can't compare {String.Join(" with ", types.Select(t => t.ToString().ToLowerInvariant()))}");
        }

        return operands;
    }

    private TypedOperand Operand(CqlExpression expression)
        => expression switch
        {
            CqlProperty property => new TypedOperand(expression, PropertyType(ResolveField(property.Name))),
            CqlString => new TypedOperand(expression, ValueType.Text),
            CqlNumber => new TypedOperand(expression, ValueType.Number),
            CqlBoolean => new TypedOperand(expression, ValueType.Boolean),
            CqlDate or CqlTimestamp => new TypedOperand(expression, ValueType.Temporal),
            CqlCaseInsensitive casei => Operand(casei.Expression).Type is ValueType.Text or ValueType.Any
                ? new TypedOperand(expression, ValueType.Text)
                : throw Error("CASEI can only be used with text"),
            _ => throw Error("unsupported expression")
        };

    private static ValueType PropertyType(IField field)
        => field switch
        {
            _ when field.IsStringType() => ValueType.Text,
            _ when field.IsIntegerType() || field.IsFloatingPointType() => ValueType.Number,
            _ when field.type == FieldType.boolean => ValueType.Boolean,
            _ when field.type == FieldType.Date => ValueType.Temporal,
            _ => ValueType.Any
        };

    private IField ResolveField(string name)
        => _resolveField(name) ?? throw Error($"unknown or not queryable property '{name}'");

    private static CqlExpression Unwrap(CqlExpression expression)
        => expression is CqlCaseInsensitive casei ? Unwrap(casei.Expression) : expression;

    #endregion

    #region Rendering

    /// <summary>
    /// Renders an operand. Temporal comparisons compare dates if any operand is a DATE literal, otherwise timestamps.
    /// </summary>
    private string Render(TypedOperand operand, IReadOnlyCollection<TypedOperand> siblings)
    {
        bool dateMode = siblings.Any(o => o.Expression is CqlDate);

        return operand.Expression switch
        {
            CqlProperty property => operand.Type == ValueType.Temporal
                ? RenderTemporalProperty(ResolveField(property.Name).name, dateMode)
                : ResolveField(property.Name).name,
            CqlString s => $"'{s.Value.Replace("'", "''")}'",
            CqlNumber n => n.Value.ToString("R", CultureInfo.InvariantCulture),
            CqlBoolean b => RenderBoolean(b.Value),
            CqlDate d => RenderDate(d.Value),
            CqlTimestamp t => dateMode ? RenderDate(DateOnly.FromDateTime(t.Value)) : RenderTimestamp(t.Value),
            CqlCaseInsensitive casei => $"UPPER({Render(Operand(casei.Expression), siblings)})",
            _ => throw Error("unsupported expression")
        };
    }

    private string RenderPropertyOnly(CqlExpression expression)
        => expression is CqlProperty property
            ? ResolveField(property.Name).name
            : throw Error("IS NULL can only be used with properties");

    private string RenderBoolean(bool value)
        => _dialect switch
        {
            SqlDialect.PostgreSql => value ? "TRUE" : "FALSE",
            SqlDialect.SqlServer or SqlDialect.SQLite => value ? "1" : "0",
            _ => throw UnsupportedByDataSource("boolean")
        };

    private string RenderDate(DateOnly value)
    {
        string date = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return _dialect switch
        {
            SqlDialect.PostgreSql => $"CAST('{date}' AS date)",
            SqlDialect.SqlServer => $"CAST('{date}' AS date)",
            SqlDialect.SQLite => $"date('{date}')",
            _ => throw UnsupportedByDataSource("date")
        };
    }

    private string RenderTimestamp(DateTime utc)
        => _dialect switch
        {
            SqlDialect.PostgreSql => $"CAST('{utc.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF", CultureInfo.InvariantCulture)}' AS timestamp)",
            SqlDialect.SqlServer => $"CONVERT(datetime2, '{utc.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture)}', 126)",
            SqlDialect.SQLite => $"datetime('{utc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}')",
            _ => throw UnsupportedByDataSource("timestamp")
        };

    /// <summary>
    /// SQLite stores dates as text in different formats => normalize with date()/datetime()
    /// </summary>
    private string RenderTemporalProperty(string fieldName, bool dateMode)
        => _dialect switch
        {
            SqlDialect.SQLite => dateMode ? $"date({fieldName})" : $"datetime({fieldName})",
            SqlDialect.PostgreSql or SqlDialect.SqlServer => dateMode ? $"CAST({fieldName} AS date)" : fieldName,
            _ => throw UnsupportedByDataSource("date")
        };

    #endregion

    private static OgcApiException UnsupportedByDataSource(string what)
        => OgcApiException.BadRequest($"Invalid CQL2 filter: {what} values are not supported for this data source");

    private static OgcApiException Error(string message)
        => OgcApiException.BadRequest($"Invalid CQL2 filter: {message}");
}
