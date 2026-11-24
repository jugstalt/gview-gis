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
