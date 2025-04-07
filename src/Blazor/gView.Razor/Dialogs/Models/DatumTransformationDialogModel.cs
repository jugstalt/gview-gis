using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Geometry;

namespace gView.Razor.Dialogs.Models;
public class DatumTransformationDialogModel : IDialogResultItem
{
    public IDatumTransformation? DatumTransformation { get; set; }
}
