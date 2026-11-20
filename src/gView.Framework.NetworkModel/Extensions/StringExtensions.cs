namespace gView.Framework.NetworkModel.Extensions;

public static class StringExtensions
{
    /// <summary>Null for null, empty and white space, otherwise the text itself.</summary>
    public static string? NullIfEmpty(this string? text) => string.IsNullOrWhiteSpace(text) ? null : text;
}
