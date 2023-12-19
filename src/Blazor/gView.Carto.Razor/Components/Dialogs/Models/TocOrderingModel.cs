using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.UI;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class TocOrderingModel : IDialogResultItem
{
    public ITocElement? SelectedGroupElement { get; set; }
    public string MapName { get; set; } = "Map";
    public IToc? Toc { get; set; }
}
