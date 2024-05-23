using gView.Carto.Core.Services.Abstraction;
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

    async public Task<object?> EditAsync(IApplicationScopeFactory scope,
                                         object instance,
                                         PropertyInfo propertyInfo)
    {
        var colorGradient = propertyInfo.GetValue(instance) as ColorGradient;
        if (colorGradient == null)
        {
            return null;
        }

        var model = await scope.ShowModalDialog(
            typeof(gView.Carto.Razor.Components.Dialogs.ColorGradientDialog),
            $"Color Gradient",
            new ColorGradientModel()
            {
                ColorGradient = colorGradient
            });

        return model?.ColorGradient;
    }
}
