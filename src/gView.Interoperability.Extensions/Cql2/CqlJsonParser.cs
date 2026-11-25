#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace gView.Interoperability.Extensions.Cql2;

/// <summary>
/// Parses CQL2-JSON (the same subset as <see cref="CqlTextParser"/>: basic CQL2, advanced comparison operators,
/// case-insensitive comparison) into the CQL2 syntax tree:
/// <c>{"op":"and","args":[{"op":"=","args":[{"property":"NAME"},"Wien"]},{"op":"&gt;","args":[{"property":"EW"},1000]}]}</c>
/// </summary>
public static class CqlJsonParser
{
    public static CqlPredicate Parse(string cql2Json)
    {
        if (String.IsNullOrWhiteSpace(cql2Json))
        {
            throw Error("empty filter");
        }

        try
        {
            using var document = JsonDocument.Parse(cql2Json);
            return Parse(document.RootElement);
        }
        catch (JsonException ex)
        {
            throw Error($"invalid JSON: {ex.Message}");
        }
    }

    public static CqlPredicate Parse(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.True => new CqlBooleanPredicate(true),
            JsonValueKind.False => new CqlBooleanPredicate(false),
            JsonValueKind.Object => ParseOperation(element),
            _ => throw Error("a predicate must be an object with \"op\" and \"args\" (or true/false)")
        };

    private static CqlPredicate ParseOperation(JsonElement element)
    {
        if (!element.TryGetProperty("op", out var opElement) || opElement.ValueKind != JsonValueKind.String)
        {
            throw Error("a predicate needs \"op\"");
        }
        string op = opElement.GetString()!.ToLowerInvariant();
        var args = element.TryGetProperty("args", out var argsElement) && argsElement.ValueKind == JsonValueKind.Array
            ? argsElement.EnumerateArray().ToList()
            : throw Error($"\"{op}\" needs \"args\" as array");

        switch (op)
        {
            case "and":
            case "or":
                if (args.Count < 2)
                {
                    throw Error($"\"{op}\" needs at least two arguments");
                }
                var predicates = args.Select(Parse).ToList();
                return predicates.Skip(1).Aggregate(predicates[0], (left, right) => op == "and" ? new CqlAnd(left, right) : new CqlOr(left, right));

            case "not":
                Count(op, args, 1);
                return new CqlNot(Parse(args[0]));

            case "=":
            case "<>":
            case "<":
            case ">":
            case "<=":
            case ">=":
                Count(op, args, 2);
                return new CqlComparison(op, Expression(args[0]), Expression(args[1]));

            case "like":
                Count(op, args, 2);
                return new CqlLike(Expression(args[0]), Expression(args[1]), false);

            case "between":
                Count(op, args, 3);
                return new CqlBetween(Expression(args[0]), Expression(args[1]), Expression(args[2]), false);

            case "in":
                Count(op, args, 2);
                if (args[1].ValueKind != JsonValueKind.Array)
                {
                    throw Error("the second argument of \"in\" must be an array");
                }
                return new CqlIn(Expression(args[0]), args[1].EnumerateArray().Select(Expression).ToList(), false);

            case "isnull":
                Count(op, args, 1);
                return new CqlIsNull(Expression(args[0]), false);

            default:
                throw Error($"unsupported operator \"{op}\"");
        }
    }

    private static CqlExpression Expression(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.String => new CqlString(element.GetString()!),
            JsonValueKind.Number => new CqlNumber(element.GetDouble()),
            JsonValueKind.True => new CqlBoolean(true),
            JsonValueKind.False => new CqlBoolean(false),
            JsonValueKind.Object when element.TryGetProperty("property", out var property) && property.ValueKind == JsonValueKind.String
                => new CqlProperty(property.GetString()!),
            JsonValueKind.Object when element.TryGetProperty("casei", out var casei)
                => new CqlCaseInsensitive(Expression(casei)),
            JsonValueKind.Object when element.TryGetProperty("date", out var date) && date.ValueKind == JsonValueKind.String
                => DateOnly.TryParseExact(date.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
                    ? new CqlDate(value)
                    : throw Error($"invalid date '{date.GetString()}'"),
            JsonValueKind.Object when element.TryGetProperty("timestamp", out var timestamp) && timestamp.ValueKind == JsonValueKind.String
                => DateTimeOffset.TryParse(timestamp.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value)
                    ? new CqlTimestamp(value.UtcDateTime)
                    : throw Error($"invalid timestamp '{timestamp.GetString()}'"),
            _ => throw Error($"unsupported value {element.GetRawText()}")
        };

    private static void Count(string op, List<JsonElement> args, int count)
    {
        if (args.Count != count)
        {
            throw Error($"\"{op}\" needs {count} argument(s)");
        }
    }

    private static CqlException Error(string message) => new($"Invalid CQL2 filter: {message}");
}
