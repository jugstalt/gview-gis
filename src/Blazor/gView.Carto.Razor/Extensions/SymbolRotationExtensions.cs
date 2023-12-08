using gView.Carto.Razor.Components.Dialogs;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;

namespace gView.Carto.Razor.Extensions;
static public class SymbolRotationExtensions
{
    async static public Task EditProperties(
        this SymbolRotation? symbolRotation,
        IApplicationScope appScope,
        ITableClass? tableClass = null)
    {
        if(symbolRotation is null)
        {
            return;
        }

        var model = await appScope.ShowModalDialog
            (typeof(SymbolRotationDialog),
            "Symbol Rotation",
            new SymbolRotationModel()
            {
                TableClass = tableClass,
                RotationType = symbolRotation.RotationType,
                RotationUnit = symbolRotation.RotationUnit,
                RotationFieldName = symbolRotation.RotationFieldName
            },
            new ModalDialogOptions()
            {
                Width = ModalDialogWidth.ExtraSmall
            });

        if (model is not null)
        {
            symbolRotation.RotationType = model.RotationType;
            symbolRotation.RotationUnit = model.RotationUnit;
            symbolRotation.RotationFieldName = model.RotationFieldName;
        }
    }
}
