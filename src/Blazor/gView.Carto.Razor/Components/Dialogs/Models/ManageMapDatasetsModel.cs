using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Carto;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class ManageMapDatasetsModel : IDialogResultItem
{
    public IMap? Map { get; set; } = null;
}
