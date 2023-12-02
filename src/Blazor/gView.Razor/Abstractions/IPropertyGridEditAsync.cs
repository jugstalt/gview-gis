using gView.Framework.Blazor.Services.Abstraction;
using System.Reflection;

namespace gView.Razor.Abstractions;

public interface IPropertyGridEditAsync : IPropertyGridEditor
{
    Task<object?> EditAsync(IApplicationScope scope,
                            object instance,
                            PropertyInfo propertyInfo);
}
