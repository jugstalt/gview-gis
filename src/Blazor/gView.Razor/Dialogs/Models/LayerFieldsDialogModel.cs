using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;

namespace gView.Razor.Dialogs.Models;
public class LayerFieldsDialogModel : IDialogResultItem
{
    public IFeatureLayer? Layer { get; set; }
}
