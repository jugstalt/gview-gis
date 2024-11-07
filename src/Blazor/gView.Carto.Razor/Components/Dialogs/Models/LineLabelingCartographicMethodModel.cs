using gView.Blazor.Models.Dialogs;
using gView.Framework.Cartography.Rendering;

namespace gView.Carto.Razor.Components.Dialogs.Models;
public class LineLabelingCartographicMethodModel : IDialogResultItem
{
    public SimpleLabelRenderer.CartographicLineLabeling CartographicLineLabeling { get; set; }
}
