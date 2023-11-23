using gView.Framework.Blazor.Services.Abstraction;

namespace gView.Razor.Abstractions;

public interface IPropertyGridEditor
{
    Type PropertyType { get; }

    Task<object?> EditAsync(IApplicationScope scope, object? propertyValue);

}
