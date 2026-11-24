using gView.Carto.Razor.Components.Controls.Renderers.Models;
using gView.Carto.Razor.Components.Dialogs;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Geometry;
using gView.Framework.Symbology.Extensions;

namespace gView.Carto.Razor.Extensions;

static internal class KeySymbolExtensions
{
    static public bool ContainsKey<TKey>(this IEnumerable<KeySymbol<TKey>> keySymbols, TKey key)
        => keySymbols.Any(k => key?.Equals(k.Key) == true);

    /// <summary>
    /// Shows the recolor dialog and recolors all (bulk editable) class symbols in list order.
    /// </summary>
    /// <returns>A new list (new item instances, so the list control refreshes) or null if cancelled</returns>
    static public async Task<List<KeySymbol<TKey>>?> RecolorAsync<TKey>(
                this List<KeySymbol<TKey>> keySymbols,
                IApplicationScopeFactory appScope,
                GeometryType geometryType,
                Func<TKey, bool>? isAllOtherValues = null)
    {
        isAllOtherValues ??= _ => false;
        var classes = keySymbols.Where(k => !isAllOtherValues(k.Key)).ToArray();

        var model = await appScope.ShowModalDialog(
            typeof(RecolorSymbolsDialog),
            "Recolor Symbols",
            new RecolorSymbolsModel()
            {
                GeometryType = geometryType,
                ClassCount = classes.Length,
                SkippedCount = classes.Count(k => !k.Symbol.CanBulkEdit()),
                HasAllOtherValues = classes.Length < keySymbols.Count
            });

        if (model is null)
        {
            return null;
        }

        var targets = keySymbols
            .Where(k => model.IncludeAllOtherValues || !isAllOtherValues(k.Key))
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
    /// Shows the set properties dialog and applies the checked properties to all (bulk editable) class symbols.
    /// </summary>
    /// <returns>A new list (new item instances, so the list control refreshes) or null if cancelled</returns>
    static public async Task<List<KeySymbol<TKey>>?> SetSymbolPropertiesAsync<TKey>(
                this List<KeySymbol<TKey>> keySymbols,
                IApplicationScopeFactory appScope,
                GeometryType geometryType,
                Func<TKey, bool>? isAllOtherValues = null)
    {
        isAllOtherValues ??= _ => false;
        var classes = keySymbols.Where(k => !isAllOtherValues(k.Key)).ToArray();

        var startModel = new SetSymbolPropertiesModel()
        {
            GeometryType = geometryType,
            ClassCount = classes.Length,
            SkippedCount = classes.Count(k => !k.Symbol.CanBulkEdit()),
            HasAllOtherValues = classes.Length < keySymbols.Count
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

        foreach (var keySymbol in keySymbols.Where(k => model.IncludeAllOtherValues || !isAllOtherValues(k.Key)))
        {
            keySymbol.Symbol.ApplyProperties(values);
        }

        return keySymbols.Select(k => k.Copy()).ToList();
    }
}
