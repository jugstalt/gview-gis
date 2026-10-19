using gView.Blazor.Models.Dialogs;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

/// <summary>The result of checking one URL: whether it can be read, and a description or the error.</summary>
public record CogUrlCheck(string Url, bool Ok, string Message);

/// <summary>
/// URLs of Cloud Optimized GeoTIFFs, one per line. The caller provides <see cref="Check"/>, which opens a URL; the dialog
/// does not close while a line fails.
/// </summary>
public class CogConnectionModel : IDialogResultItem
{
    public string Urls { get; set; } = string.Empty;

    /// <summary>Only one URL is allowed (when an existing connection is edited).</summary>
    public bool Single { get; set; }

    public Func<string, Task<CogUrlCheck>>? Check { get; set; }

    /// <summary>The results of the last check, in the order of the lines.</summary>
    public List<CogUrlCheck> Results { get; } = new();

    /// <summary>The non-empty lines.</summary>
    public IEnumerable<string> Lines => Urls
        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(line => line.Length > 0);
}
