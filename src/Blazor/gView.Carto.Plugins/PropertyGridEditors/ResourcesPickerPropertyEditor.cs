using gView.Carto.Plugins.Extensions;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Razor.Abstractions;
using System.Reflection;

namespace gView.Carto.Plugins.PropertyGridEditors;

internal class ResourcesPickerPropertyEditor : IPropertyGridEditAsync
{
    public Type PropertyType => typeof(FileInfo);

    async public Task<object?> EditAsync(IApplicationScope scope,
                                   object instance,
                                   PropertyInfo propertyInfo)
    {
        var scopeService = scope.ToCartoScopeService();

        var model = await scopeService.ShowModalDialog(
            typeof(gView.Carto.Razor.Components.Dialogs.ListSelectorDialog<string>),
            $"Color Gradient",
            new ListSelectorModel<string>()
            {
                Items = scopeService.Document?.Map?.ResourceContainer?.Names ?? []
            });

        var resourceName = model?.Result.SelectedItem;

        return propertyInfo.PropertyType switch
        {
            Type t when t == typeof(string) && !String.IsNullOrEmpty(resourceName)
                => $"resource:{resourceName}",
            _ => null
        };
    }
}
