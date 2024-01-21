using gView.Blazor.Models.DataTable;
using gView.Framework.Core.Data;

namespace gView.Blazor.Core.Extensions;

public static class DataModeExtensions
{
    public static string IconString(this Mode mode)
        => mode switch
        {
            Mode.Selection => "webgis:marker",
            Mode.Identify => "webgis:identify",
            _ => "basic:table"
        };

    public static bool AsApplicable(this Mode mode, DataTableProperties tableProperties)
        => mode switch
        {
            Mode.Identify => tableProperties.IdentifyFilter is not null,
            _ => true
        };

    static public string Title(this Mode mode, ILayer? layer)
        => mode switch
        {
            Mode.Selection when layer is IFeatureSelection fSelection => $"{mode.ToString().SplitCamelCase()} ({fSelection.SelectionSet?.Count ?? 0})",
            _ => mode.ToString().SplitCamelCase()
        };
}
