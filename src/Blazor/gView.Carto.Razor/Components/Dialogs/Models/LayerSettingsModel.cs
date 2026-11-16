using gView.Blazor.Models.Dialogs;
using gView.Framework.Cartography;
using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class LayerSettingsModel : IDialogResultItem
{
    public Map? Map { get; set; } = null;
    public ILayer? Layer { get; set; } = null;

    /// <summary>The network settings of the layer (a working copy), null when the map has no network module or the layer cannot take part.</summary>
    public NetworkLayerSettings? Network { get; set; } = null;

    public LayerSettingsDialog.Page StartPage { get; set; } = LayerSettingsDialog.Page.General;
}
