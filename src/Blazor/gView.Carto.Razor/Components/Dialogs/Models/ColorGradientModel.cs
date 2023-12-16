using gView.Blazor.Models.Dialogs;
using gView.Framework.Symbology;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class ColorGradientModel : IDialogResultItem
{
    public ColorGradient? ColorGradient { get; set; }
}
