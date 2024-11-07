using gView.Blazor.Models.Dialogs;
using gView.Blazor.Models.Settings;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class OpenPreviousMapDialogModel : IDialogResultItem
{
    public IEnumerable<MapFileItem>? Items { get; set; }

    public MapFileItem? Selected { get; set; }
}
