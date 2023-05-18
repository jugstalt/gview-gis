using gView.Blazor.Models.Dialogs;
using gView.Framework.Geometry;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class SpatialReferenceSelectorModel : IDialogResultItem
{
    public ISpatialReference? SpatialReference { get; set; }
}
