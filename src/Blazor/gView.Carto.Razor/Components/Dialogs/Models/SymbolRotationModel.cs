using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;

namespace gView.Carto.Razor.Components.Dialogs.Models;
public class SymbolRotationModel : IDialogResultItem
{
    public ITableClass? TableClass { get; set; }
    public RotationType RotationType { get; set; }
    public RotationUnit RotationUnit { get; set; }
    public string RotationFieldName { get; set; } = "";
}
