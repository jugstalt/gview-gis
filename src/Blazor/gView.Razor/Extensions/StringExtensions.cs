namespace gView.Razor.Extensions;
internal static class StringExtensions
{
    public static string AppendStyle(this string styles, string style, string value)
        => value switch
        {
            string when !String.IsNullOrEmpty(value) => $"{styles.AppendSemicolonIfNotEmpty()}{style}:{value}",
            _ => styles
        };

    public static string AppendBackgroundColor(this string styles, string value)
        => styles.AppendStyle("background-color", value);

    public static string AppendColor(this string styles, string value)
        => styles.AppendStyle("color", value);

    private static string AppendSemicolonIfNotEmpty(this string styles)
        => String.IsNullOrEmpty(styles) 
            ? "" 
            : styles.EndsWith(";") 
                ? styles 
                : $"{styles};";
}
