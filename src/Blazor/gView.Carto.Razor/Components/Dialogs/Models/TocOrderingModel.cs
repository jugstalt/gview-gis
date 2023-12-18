using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.UI;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class TocOrderingModel : IDialogResultItem
{
    public string MapName { get; set; } = "Map";
    public IToc? Toc { get; set; }
}
