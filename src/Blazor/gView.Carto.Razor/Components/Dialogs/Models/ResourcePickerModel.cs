using gView.Blazor.Models.Dialogs;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class ResourcePickerModel : IDialogResultItem
{
    /// <summary>
    /// Candidate map resources (already filtered by the caller to the extensions
    /// relevant for the property being edited), keyed by resource name.
    /// </summary>
    public IReadOnlyDictionary<string, byte[]> Resources { get; set; } = new Dictionary<string, byte[]>();

    public ResultClass Result { get; set; } = new();

    public class ResultClass
    {
        public string? SelectedItem { get; set; }
    }
}
