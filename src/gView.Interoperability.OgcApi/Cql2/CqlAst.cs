namespace gView.Interoperability.OgcApi.Cql2;

/// <summary>
/// Boolean expressions of the CQL2 (basic + advanced comparison + case-insensitive comparison) syntax tree
/// </summary>
public abstract record CqlPredicate;

public sealed record CqlAnd(CqlPredicate Left, CqlPredicate Right) : CqlPredicate;

public sealed record CqlOr(CqlPredicate Left, CqlPredicate Right) : CqlPredicate;

public sealed record CqlNot(CqlPredicate Predicate) : CqlPredicate;

/// <summary>
/// TRUE/FALSE as a whole predicate
/// </summary>
public sealed record CqlBooleanPredicate(bool Value) : CqlPredicate;

public sealed record CqlComparison(string Operator, CqlExpression Left, CqlExpression Right) : CqlPredicate;

public sealed record CqlLike(CqlExpression Value, CqlExpression Pattern, bool Negated) : CqlPredicate;

public sealed record CqlBetween(CqlExpression Value, CqlExpression Lower, CqlExpression Upper, bool Negated) : CqlPredicate;

public sealed record CqlIn(CqlExpression Value, IReadOnlyList<CqlExpression> List, bool Negated) : CqlPredicate;

public sealed record CqlIsNull(CqlExpression Value, bool Negated) : CqlPredicate;

/// <summary>
/// Scalar expressions
/// </summary>
public abstract record CqlExpression;

public sealed record CqlProperty(string Name) : CqlExpression;

public sealed record CqlString(string Value) : CqlExpression;

public sealed record CqlNumber(double Value) : CqlExpression;

public sealed record CqlBoolean(bool Value) : CqlExpression;

public sealed record CqlDate(DateOnly Value) : CqlExpression;

/// <summary>
/// TIMESTAMP('...'), always UTC
/// </summary>
public sealed record CqlTimestamp(DateTime Value) : CqlExpression;

public sealed record CqlCaseInsensitive(CqlExpression Expression) : CqlExpression;
