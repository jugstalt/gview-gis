using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Reflection;
using gView.Razor.Abstractions;
using System.Reflection;

namespace gView.Carto.Plugins.PropertyGridEditors;

internal class ResourcesPickerPropertyEditor : IPropertyGridEditAsync
{
    public Type PropertyType => typeof(FileInfo);

    async public Task<object?> EditAsync(IApplicationScopeFactory scope,
                                   object instance,
                                   PropertyInfo propertyInfo)
    {
        var service = scope.GetApplicationScope<ICartoApplicationScopeService>();
        var resourceContainer = service.Document?.Map?.ResourceContainer;

        var extensions = propertyInfo.GetCustomAttribute<PropertyDescriptionAttribute>()?
            .FileExtensions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        var resourceNames = resourceContainer?.Names ?? [];
        if (extensions.Length > 0)
        {
            resourceNames = resourceNames.Where(name =>
                extensions.Any(ext => name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
        }

        var resources = resourceNames.ToDictionary(
            name => name,
            name => resourceContainer?[name] ?? []);

        var model = await scope.ShowModalDialog(
            typeof(gView.Carto.Razor.Components.Dialogs.ResourcePickerDialog),
            $"Select Resource",
            new ResourcePickerModel()
            {
                Resources = resources
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
