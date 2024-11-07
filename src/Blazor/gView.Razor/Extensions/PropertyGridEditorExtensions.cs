using gView.Razor.Abstractions;

namespace gView.Razor.Extensions;

internal static class PropertyGridEditorExtensions
{
    static public IPropertyGridEditor? FirstEditorOrNull(this IEnumerable<IPropertyGridEditor> editors, Type? type)
        => type is null
                ? null
                : editors?.FirstOrDefault(e =>
                    e.PropertyType == type || type.IsAssignableTo(e.PropertyType));
}
