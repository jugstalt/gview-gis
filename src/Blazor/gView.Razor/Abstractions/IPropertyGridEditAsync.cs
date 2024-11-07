using gView.Framework.Blazor.Services.Abstraction;
using System.Reflection;

namespace gView.Razor.Abstractions;

public interface IPropertyGridEditAsync : IPropertyGridEditor
{
    Task<object?> EditAsync(IApplicationScopeFactory scope,
                            object instance,
                            PropertyInfo propertyInfo);
}
