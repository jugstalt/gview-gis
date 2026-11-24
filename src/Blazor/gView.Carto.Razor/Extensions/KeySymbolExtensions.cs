using gView.Carto.Razor.Components.Controls.Renderers.Models;
using gView.Carto.Razor.Components.Dialogs;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Extensions;

namespace gView.Carto.Razor.Extensions;

static internal class KeySymbolExtensions
{
    static public bool ContainsKey<TKey>(this IEnumerable<KeySymbol<TKey>> keySymbols, TKey key)
        => keySymbols.Any(k => key?.Equals(k.Key) == true);

    /// <summary>
    /// The selected key symbols in list order (keys that no longer exist are ignored).
    /// </summary>
    static public KeySymbol<TKey>[] SelectedKeySymbols<TKey>(this IEnumerable<KeySymbol<TKey>> keySymbols, ICollection<TKey>? selectedKeys)
        => selectedKeys is null || selectedKeys.Count == 0
            ? []
            : keySymbols.Where(k => selectedKeys.Contains(k.Key)).ToArray();

    /// <summary>
    /// Shows the recolor dialog and recolors the (bulk editable) class symbols in list order:
    /// the selected classes, or all classes if nothing is selected.
    /// </summary>
    /// <returns>A new list (new item instances, so the list control refreshes) or null if cancelled</returns>
    static public async Task<List<KeySymbol<TKey>>?> RecolorAsync<TKey>(
                this List<KeySymbol<TKey>> keySymbols,
                IApplicationScopeFactory appScope,
                GeometryType geometryType,
                Func<TKey, bool>? isAllOtherValues = null,
                ICollection<TKey>? selectedKeys = null)
    {
        isAllOtherValues ??= _ => false;
        var selected = keySymbols.SelectedKeySymbols(selectedKeys);
        var classes = selected.Length > 0
            ? selected
            : keySymbols.Where(k => !isAllOtherValues(k.Key)).ToArray();

        var model = await appScope.ShowModalDialog(
            typeof(RecolorSymbolsDialog),
            "Recolor Symbols",
            new RecolorSymbolsModel()
            {
                GeometryType = geometryType,
                ClassCount = classes.Length,
                SkippedCount = classes.Count(k => !k.Symbol.CanBulkEdit()),
                HasAllOtherValues = selected.Length == 0 && classes.Length < keySymbols.Count,
                SelectedCount = selected.Length
            });

        if (model is null)
        {
            return null;
        }

        var targets = keySymbols
            .Where(k => selected.Length > 0
                        ? selected.Contains(k)
                        : model.IncludeAllOtherValues || !isAllOtherValues(k.Key))
            .Where(k => k.Symbol.CanBulkEdit())
            .ToArray();

        var colors = model.GetColors(targets.Length);
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].Symbol.ApplyColor(colors[i], model.Target);
        }

        return keySymbols.Select(k => k.Copy()).ToList();
    }

    /// <summary>
    /// Shows the set properties dialog and applies the checked properties to the (bulk editable)
    /// class symbols: the selected classes, or all classes if nothing is selected.
    /// </summary>
    /// <returns>A new list (new item instances, so the list control refreshes) or null if cancelled</returns>
    static public async Task<List<KeySymbol<TKey>>?> SetSymbolPropertiesAsync<TKey>(
                this List<KeySymbol<TKey>> keySymbols,
                IApplicationScopeFactory appScope,
                GeometryType geometryType,
                Func<TKey, bool>? isAllOtherValues = null,
                ICollection<TKey>? selectedKeys = null)
    {
        isAllOtherValues ??= _ => false;
        var selected = keySymbols.SelectedKeySymbols(selectedKeys);
        var classes = selected.Length > 0
            ? selected
            : keySymbols.Where(k => !isAllOtherValues(k.Key)).ToArray();

        var startModel = new SetSymbolPropertiesModel()
        {
            GeometryType = geometryType,
            ClassCount = classes.Length,
            SkippedCount = classes.Count(k => !k.Symbol.CanBulkEdit()),
            HasAllOtherValues = selected.Length == 0 && classes.Length < keySymbols.Count,
            SelectedCount = selected.Length
        };
        startModel.SetStartValues(
            classes.FirstOrDefault(k => k.Symbol.CanBulkEdit())?.Symbol.ReadPropertyValues()
            ?? new());

        var model = await appScope.ShowModalDialog(
            typeof(SetSymbolPropertiesDialog),
            "Set Symbol Properties",
            startModel);

        var values = model?.ToValues();
        if (model is null || values is null || values.IsEmpty)
        {
            return null;
        }

        foreach (var keySymbol in keySymbols.Where(k => selected.Length > 0
                                                        ? selected.Contains(k)
                                                        : model.IncludeAllOtherValues || !isAllOtherValues(k.Key)))
        {
            keySymbol.Symbol.ApplyProperties(values);
        }

        return keySymbols.Select(k => k.Copy()).ToList();
    }

    /// <summary>
    /// Asks for the order (label, value or feature count; count only if counted) and returns the ordered list.
    /// The "all other values" class stays at the end.
    /// </summary>
    /// <returns>A new list (new item instances, so the list control refreshes) or null if cancelled</returns>
    static public async Task<List<KeySymbol<string>>?> OrderByDialogAsync(
                this List<KeySymbol<string>> keySymbols,
                IApplicationScopeFactory appScope,
                IReadOnlyDictionary<string, long>? counts,
                Func<string, bool>? isAllOtherValues = null)
    {
        var options = new List<KeyValuePair<string, KeySymbolOrder>>()
        {
            new("Label (A-Z)", KeySymbolOrder.LabelAscending),
            new("Label (Z-A)", KeySymbolOrder.LabelDescending),
            new("Value (ascending)", KeySymbolOrder.ValueAscending),
            new("Value (descending)", KeySymbolOrder.ValueDescending)
        };
        if (counts is not null)
        {
            options.Add(new("Feature count (most first)", KeySymbolOrder.CountDescending));
            options.Add(new("Feature count (fewest first)", KeySymbolOrder.CountAscending));
        }

        var model = await appScope.ShowModalDialog(
                typeof(gView.Razor.Dialogs.PromptSelectDialog<KeySymbolOrder>),
                "Order Classes",
                new gView.Razor.Dialogs.Models.PromptSelectDialogModel<KeySymbolOrder>()
                {
                    Options = options,
                    Required = true,
                    Prompt = "Order by",
                    SelectedValue = KeySymbolOrder.LabelAscending
                });

        if (model is null)
        {
            return null;
        }

        return keySymbols.OrderKeySymbols(model.SelectedValue, counts, isAllOtherValues);
    }

    static public List<KeySymbol<string>> OrderKeySymbols(
                this IEnumerable<KeySymbol<string>> keySymbols,
                KeySymbolOrder order,
                IReadOnlyDictionary<string, long>? counts,
                Func<string, bool>? isAllOtherValues = null)
    {
        isAllOtherValues ??= _ => false;
        long CountOf(KeySymbol<string> k) => counts?.TryGetValue(k.Key, out long c) == true ? c : 0;

        var classes = keySymbols.Where(k => !isAllOtherValues(k.Key));
        var ordered = order switch
        {
            KeySymbolOrder.LabelAscending => classes.OrderBy(k => k.LegendLabel, NaturalComparer.Instance),
            KeySymbolOrder.LabelDescending => classes.OrderByDescending(k => k.LegendLabel, NaturalComparer.Instance),
            KeySymbolOrder.ValueAscending => classes.OrderBy(k => k.Key, NaturalComparer.Instance),
            KeySymbolOrder.ValueDescending => classes.OrderByDescending(k => k.Key, NaturalComparer.Instance),
            KeySymbolOrder.CountDescending => classes.OrderByDescending(CountOf),
            KeySymbolOrder.CountAscending => classes.OrderBy(CountOf),
            _ => classes
        };

        return ordered
                .Concat(keySymbols.Where(k => isAllOtherValues(k.Key)))
                .Select(k => k.Copy())  // new instances, so the list control refreshes
                .ToList();
    }

    /// <summary>
    /// Compares numbers as numbers (2 &lt; 10), everything else as text (ignoring case).
    /// </summary>
    private class NaturalComparer : IComparer<string?>
    {
        public static readonly NaturalComparer Instance = new();

        public int Compare(string? x, string? y)
        {
            bool xIsNumber = TryParse(x, out double dx), yIsNumber = TryParse(y, out double dy);

            return (xIsNumber, yIsNumber) switch
            {
                (true, true) => dx.CompareTo(dy),
                (true, false) => -1,   // numbers first
                (false, true) => 1,
                _ => StringComparer.OrdinalIgnoreCase.Compare(x, y)
            };
        }

        private static bool TryParse(string? s, out double d)
            => double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d);
    }

    /// <summary>
    /// Replaces the symbols of the target classes with clones of <paramref name="template"/>
    /// (legend labels are kept; with <paramref name="keepColors"/> also the colours of each class).
    /// The replaced symbols are released.
    /// </summary>
    /// <param name="noColorCount">Number of target classes whose colours could not be kept</param>
    /// <returns>A new list (new item instances, so the list control refreshes)</returns>
    static public List<KeySymbol<TKey>> PasteSymbol<TKey>(
                this List<KeySymbol<TKey>> keySymbols,
                ISymbol template,
                IEnumerable<KeySymbol<TKey>> targets,
                bool keepColors,
                out int noColorCount)
    {
        noColorCount = 0;
        var targetSet = targets.ToHashSet();

        foreach (var keySymbol in keySymbols.Where(targetSet.Contains))
        {
            var oldSymbol = keySymbol.Symbol;
            keySymbol.Symbol = template.CreatePasteSymbol(oldSymbol, keepColors, out bool colorsKept);

            if (keepColors && !colorsKept)
            {
                noColorCount++;
            }

            if (!ReferenceEquals(oldSymbol, template))
            {
                oldSymbol?.Release();
            }
        }

        return keySymbols.Select(k => k.Copy()).ToList();
    }
}
