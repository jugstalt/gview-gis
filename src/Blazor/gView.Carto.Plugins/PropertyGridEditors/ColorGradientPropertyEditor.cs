using gView.Carto.Plugins.Extensions;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Symbology;
using gView.Razor.Abstractions;
using System.Reflection;

namespace gView.Carto.Plugins.PropertyGridEditors;

internal class ColorGradientPropertyEditor : IPropertyGridEditAsync
{
    public Type PropertyType => typeof(ColorGradient);

    async public Task<object?> EditAsync(IApplicationScope scope,
                                         object instance,
                                         PropertyInfo propertyInfo)
    {
        var scopeService = scope.ToCartoScopeService();

        var colorGradient = propertyInfo.GetValue(instance) as ColorGradient;
        if (colorGradient == null)
        {
            return null;
        }

        var model = await scopeService.ShowModalDialog(
            typeof(gView.Carto.Razor.Components.Dialogs.ColorGradientDialog),
            $"Color Gradient",
            new ColorGradientModel()
            {
                ColorGradient = colorGradient
            });

        return model?.ColorGradient;
    }
}
