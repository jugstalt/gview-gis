using gView.Razor.Abstractions;

namespace gView.Razor.Extensions;

internal static class PropertyGridEditorExtensions
{
    static public IPropertyGridEditor? FirstEditorOrNull(this IEnumerable<IPropertyGridEditor> editors, Type type)
        => editors?.FirstOrDefault(e =>
                e.PropertyType == type || type.IsAssignableTo(e.PropertyType));
}
