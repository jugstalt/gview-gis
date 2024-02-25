using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Carto;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class MatchGeoServiceLayerIdsModel : IDialogResultItem
{
    public IMap? Map { get; set; }
}
