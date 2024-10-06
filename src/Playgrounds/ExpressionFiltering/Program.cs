// Ihr Dictionary
using ExpressionFiltering.ExpressionParser;

IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { "NAME", "objectX" },
            { "FIELD1", 10 },
            { "FIELD2", 5 }
        };

// Ihre komplexe Filterbedingung
string filterCondition = "(NAME == 'objectX' && FIELD1 < FIELD2) || FIELD1 == 10";

try
{
    var predicate = SafeFilterEvaluator.BuildSafePredicate(filterCondition);
    bool isMatch = predicate(dict);
    Console.WriteLine(isMatch); // Gibt 'True' aus, wenn die Bedingung erfüllt ist
}
catch (Exception ex)
{
    Console.WriteLine($"Fehler bei der Auswertung der Bedingung: {ex.Message}");
}