using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Components.Dialogs.Models;
public class SelectMapLayerDialogModel : IDialogResultItem
{
    public IMap? Map { get; set; }
    public Func<ILayer?, bool>? LayerFilter { get; set; }

    public IEnumerable<ILayer>? SelectedLayers { get; set; }
    public ILayer? SelectedLayer => SelectedLayers?.FirstOrDefault();
}
