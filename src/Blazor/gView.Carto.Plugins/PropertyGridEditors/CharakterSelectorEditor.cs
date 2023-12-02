using gView.Carto.Plugins.Extensions;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Symbology.Models;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using gView.Razor.Abstractions;
using System.Reflection;

namespace gView.Carto.Plugins.PropertyGridEditors;

internal class CharakterSelectorEditor : IPropertyGridEditAsync
{
    public Type PropertyType => typeof(Charakter);

    async public Task<object?> EditAsync(IApplicationScope scope,
                                         object instance,
                                         PropertyInfo propertyInfo)
    {
        var scopeService = scope.ToCartoScopeService();

        var charakter = propertyInfo.GetValue(instance) as Charakter;
        if (charakter == null)
        {
            return null;
        }

        var fontPropertyInfo = instance.GetType().GetProperties()
                                                 .Where(p => p.PropertyType == typeof(IFont))
                                                 .FirstOrDefault();

        var fontName = (fontPropertyInfo?.GetValue(instance) as IFont)?.Name ??
                        Current.Engine.GetDefaultFontName();

        var model = await scopeService.ShowModalDialog(
            typeof(gView.Carto.Razor.Components.Dialogs.CharakterSelectorDialog),
            "Select Charakter",
            new CharakterSelectorModel()
            {
                FontName = fontName,
                Charakter = charakter
            });

        return model?.Charakter;

    }
}