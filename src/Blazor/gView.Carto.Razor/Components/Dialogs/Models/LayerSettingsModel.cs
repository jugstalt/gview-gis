using gView.Blazor.Models.Dialogs;
using gView.Framework.Cartography;
using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class LayerSettingsModel : IDialogResultItem
{
    public Map? Map { get; set; } = null;
    public ILayer? Layer { get; set; } = null;
}
