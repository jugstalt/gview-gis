using gView.Blazor.Models.Dialogs;
using gView.GraphicsEngine;

namespace gView.Razor.Dialogs.Models;

public class ColorPickerDialogModel : IDialogResultItem
{
    public ArgbColor ArgbColor { get; set; }
}
