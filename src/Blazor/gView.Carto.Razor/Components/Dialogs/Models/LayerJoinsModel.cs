using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class LayerJoinsModel : IDialogResultItem
{
    public IMap? Map { get; set; } = null;
    public IFeatureLayer? FeatureLayer { get; set; } = null;
}
