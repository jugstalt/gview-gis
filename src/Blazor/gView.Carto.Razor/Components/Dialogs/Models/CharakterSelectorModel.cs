using gView.Blazor.Models.Dialogs;
using gView.Framework.Symbology.Models;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class CharakterSelectorModel : IDialogResultItem
{
    public string FontName { get; set; } = "";
    public Charakter? Charakter { get; set; } = null;
}
