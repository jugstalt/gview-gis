using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class NewSqliteFdbModel : IDialogResultItem
{
    public string Name { get; set; } = "";

    /// <summary>Target folder - used by the dialog to reject a name whose file already exists.</summary>
    public string Directory { get; set; } = "";

    /// <summary>
    /// Whole-file geometry storage. Fixes the file extension (<c>.fdb</c> / <c>.fdb.gpkg</c> /
    /// <c>.fdb.sqlite</c>) and the storage of every dataset in the file.
    /// </summary>
    public GeometryStorageType Storage { get; set; } = GeometryStorageType.GeoPackage;

    public GeometryStorageType[] AllowedStorages { get; set; } = new[]
    {
        GeometryStorageType.GeoPackage,
        GeometryStorageType.SpatiaLite,
        GeometryStorageType.Classic,
    };

    /// <summary>SpatiaLite (*.fdb.sqlite) needs a deployed mod_spatialite; GeoPackage / Classic do not.</summary>
    public bool SpatiaLiteAvailable { get; set; } = true;
}
