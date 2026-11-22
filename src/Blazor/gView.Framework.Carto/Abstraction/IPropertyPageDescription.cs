namespace gView.Framework.Carto.Abstraction;

/// <summary>
/// Optional for an <see cref="IPropertyPageDefinition"/>: how the page is listed, e.g. in the service settings
/// ("NetworkServer" with a network icon instead of the name of the module class).
/// </summary>
public interface IPropertyPageDescription
{
    /// <summary>The title in the list of pages.</summary>
    string Title { get; }

    /// <summary>The icon, e.g. basic:share.</summary>
    string Icon { get; }
}
