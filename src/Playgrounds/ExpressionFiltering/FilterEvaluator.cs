//using System.Linq.Expressions;
//using System.Reflection;

//public class FilterEvaluator
//{
//    public static Func<IDictionary<string, object>, bool> BuildPredicate(string filterCondition)
//    {
//        // In diesem einfachen Parser nehmen wir an, dass die Filterbedingung die Form "Feld Operator Wert" hat,
//        // z.B. "NAME == 'objectX'" oder "FIELD1 > FIELD2"

//        // Filterbedingung in Tokens aufteilen
//        string[] tokens = filterCondition.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//        if (tokens.Length != 3)
//            throw new ArgumentException("Ungültige Filterbedingung. Erwartetes Format: 'Feld Operator Wert'.");

//        string left = tokens[0];
//        string op = tokens[1];
//        string right = tokens[2];

//        // Parameter für das Dictionary erstellen
//        ParameterExpression param = Expression.Parameter(typeof(IDictionary<string, object>), "dict");

//        // Ausdruck für das linke Operand (Feld aus dem Dictionary)
//        Expression leftExpr = GetValueExpression(param, left);

//        // Ausdruck für das rechte Operand (Wert oder anderes Feld)
//        Expression rightExpr = GetRightExpression(param, right);

//        // Vergleichsoperator bestimmen
//        Expression comparison = GetComparisonExpression(op, leftExpr, rightExpr);

//        // Lambda-Ausdruck erstellen
//        var lambda = Expression.Lambda<Func<IDictionary<string, object>, bool>>(comparison, param);

//        // Kompilieren und zurückgeben
//        return lambda.Compile();
//    }

//    private static Expression GetValueExpression(ParameterExpression param, string key)
//    {
//        // Ausdruck für dict[key]
//        MethodCallExpression getItem = Expression.Call(
//            param,
//            typeof(IDictionary<string, object>).GetMethod("get_Item"),
//            Expression.Constant(key));

//        // Konvertierung zu Object
//        return Expression.Convert(getItem, typeof(object));
//    }

//    private static Expression GetRightExpression(ParameterExpression param, string token)
//    {
//        if (token.StartsWith("'") && token.EndsWith("'"))
//        {
//            // Konstante Zeichenfolge
//            string stringValue = token.Trim('\'');
//            return Expression.Constant(stringValue);
//        }
//        else if (double.TryParse(token, out double numberValue))
//        {
//            // Numerischer Wert
//            return Expression.Constant(numberValue);
//        }
//        else
//        {
//            // Anderes Feld aus dem Dictionary
//            return GetValueExpression(param, token);
//        }
//    }

//    private static Expression GetComparisonExpression(string op, Expression left, Expression right)
//    {
//        // Beide Operanden in IComparable konvertieren
//        left = Expression.Convert(left, typeof(IComparable));
//        right = Expression.Convert(right, typeof(object));

//        // Expression für left.CompareTo(right)
//        MethodInfo compareToMethod = typeof(IComparable).GetMethod("CompareTo", new Type[] { typeof(object) });
//        Expression compareToCall = Expression.Call(left, compareToMethod, right);

//        // Konstante '0' zum Vergleich
//        Expression zero = Expression.Constant(0, typeof(int));

//        switch (op)
//        {
//            case "==":
//                return Expression.Equal(compareToCall, zero);
//            case "!=":
//                return Expression.NotEqual(compareToCall, zero);
//            case ">":
//                return Expression.GreaterThan(compareToCall, zero);
//            case "<":
//                return Expression.LessThan(compareToCall, zero);
//            case ">=":
//                return Expression.GreaterThanOrEqual(compareToCall, zero);
//            case "<=":
//                return Expression.LessThanOrEqual(compareToCall, zero);
//            default:
//                throw new NotSupportedException($"Operator '{op}' wird nicht unterstützt.");
//        }
//    }

//    private static Expression GetComparisonExpression__(string op, Expression left, Expression right)
//    {
//        // Beide Operanden in einen gemeinsamen Typ konvertieren
//        left = Expression.Convert(left, typeof(IComparable));
//        right = Expression.Convert(right, typeof(IComparable));

//        switch (op)
//        {
//            case "==":
//                return Expression.Equal(left, right);
//            case "!=":
//                return Expression.NotEqual(left, right);
//            case ">":
//                return Expression.GreaterThan(left, right);
//            case "<":
//                return Expression.LessThan(left, right);
//            case ">=":
//                return Expression.GreaterThanOrEqual(left, right);
//            case "<=":
//                return Expression.LessThanOrEqual(left, right);
//            default:
//                throw new NotSupportedException($"Operator '{op}' wird nicht unterstützt.");
//        }
//    }
//}
