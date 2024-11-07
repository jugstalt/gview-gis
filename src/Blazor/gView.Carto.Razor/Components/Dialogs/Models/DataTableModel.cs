using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class DataTableModel : IDialogResultItem
{
    public ILayer? Layer { get; set; }
}
