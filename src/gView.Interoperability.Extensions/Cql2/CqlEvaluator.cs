#nullable enable

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace gView.Interoperability.Extensions.Cql2;

/// <summary>
/// Evaluates a CQL2 predicate in memory, e.g. on the rows of a statistics result (having). Comparisons with NULL are
/// false (as in SQL); numbers compare as numbers, texts ordinal, dates/timestamps as points in time.
/// </summary>
public static class CqlEvaluator
{
    /// <param name="value">the value of a property (null for NULL); unknown properties throw a <see cref="CqlException"/></param>
    public static bool Evaluate(CqlPredicate predicate, Func<string, object?> value)
        => predicate switch
        {
            CqlAnd and => Evaluate(and.Left, value) && Evaluate(and.Right, value),
            CqlOr or => Evaluate(or.Left, value) || Evaluate(or.Right, value),
            CqlNot not => !Evaluate(not.Predicate, value),
            CqlBooleanPredicate b => b.Value,
            CqlIsNull isNull => (Value(isNull.Value, value) is null) != isNull.Negated,
            CqlComparison comparison => Compare(Value(comparison.Left, value), Value(comparison.Right, value)) is int order && comparison.Operator switch
            {
                "=" => order == 0,
                "<>" => order != 0,
                "<" => order < 0,
                ">" => order > 0,
                "<=" => order <= 0,
                ">=" => order >= 0,
                _ => throw new CqlException($"Invalid CQL2 filter: unsupported operator '{comparison.Operator}'")
            },
            CqlLike like => Value(like.Value, value) is object text && Value(like.Pattern, value) is object pattern
                            && LikeRegex(Convert.ToString(pattern, CultureInfo.InvariantCulture)!).IsMatch(Convert.ToString(text, CultureInfo.InvariantCulture)!) != like.Negated,
            CqlBetween between => Value(between.Value, value) is object v
                                  && Compare(v, Value(between.Lower, value)) is >= 0
                                  && Compare(v, Value(between.Upper, value)) is <= 0
                                  ? !between.Negated
                                  : between.Negated && Value(between.Value, value) is not null,
            CqlIn @in => Value(@in.Value, value) is object v
                         && @in.List.Any(item => Compare(v, Value(item, value)) == 0) != @in.Negated,
            _ => throw new CqlException("Invalid CQL2 filter: unsupported predicate")
        };

    private static object? Value(CqlExpression expression, Func<string, object?> value)
        => expression switch
        {
            CqlProperty property => value(property.Name) is DBNull ? null : value(property.Name),
            CqlString s => s.Value,
            CqlNumber n => n.Value,
            CqlBoolean b => b.Value,
            CqlDate d => d.Value.ToDateTime(TimeOnly.MinValue),
            CqlTimestamp t => t.Value,
            CqlCaseInsensitive casei => Value(casei.Expression, value) is object v
                                        ? Convert.ToString(v, CultureInfo.InvariantCulture)!.ToLowerInvariant()
                                        : null,
            _ => throw new CqlException("Invalid CQL2 filter: unsupported expression")
        };

    /// <summary>null, if one side is NULL or the values can't be compared</summary>
    private static int? Compare(object? left, object? right)
    {
        if (left is null || right is null)
        {
            return null;
        }
        if (IsNumber(left) && IsNumber(right))
        {
            return Convert.ToDouble(left, CultureInfo.InvariantCulture).CompareTo(Convert.ToDouble(right, CultureInfo.InvariantCulture));
        }
        if (left is DateTime leftTime && right is DateTime rightTime)
        {
            return leftTime.CompareTo(rightTime);
        }
        if (left is bool leftBool && right is bool rightBool)
        {
            return leftBool.CompareTo(rightBool);
        }
        return String.CompareOrdinal(Convert.ToString(left, CultureInfo.InvariantCulture), Convert.ToString(right, CultureInfo.InvariantCulture));
    }

    private static bool IsNumber(object value)
        => value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;

    /// <summary>% any text, _ one character, \ escapes</summary>
    private static Regex LikeRegex(string pattern)
    {
        var sb = new StringBuilder("^");
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            if (c == '\\' && i + 1 < pattern.Length)
            {
                sb.Append(Regex.Escape(pattern[++i].ToString()));
            }
            else
            {
                sb.Append(c switch
                {
                    '%' => ".*",
                    '_' => ".",
                    _ => Regex.Escape(c.ToString())
                });
            }
        }
        return new Regex(sb.Append('$').ToString(), RegexOptions.Singleline);
    }
}
