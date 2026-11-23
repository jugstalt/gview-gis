using gView.Pipe.Abstraction.Credentials;
using gView.Pipe.Abstraction.Nodes;
using System.ComponentModel.DataAnnotations;

namespace gView.Pipe.Plugins.Gis;

public enum GisDatasourceType
{
    [PipeEnumValue("PostgreSQL with PostGIS, e.g. server=db;port=5432;database=gis;user id=pipe;password=…", Label = "PostGIS")]
    PostGis,

    [PipeEnumValue("SQL Server geometry columns, e.g. Server=db;Database=gis;Trusted_Connection=True", Label = "SQL Server (geometry)")]
    SqlServerGeometry,

    [PipeEnumValue("An Esri SDE geodatabase on SQL Server", Label = "SDE (SQL Server)")]
    SdeSqlServer,

    [PipeEnumValue("A folder of shape files, e.g. D:\\data\\shapes; read and insert only, gView can't update or delete in shape files", Label = "Shape files")]
    ShapeFiles,

    [PipeEnumValue("A GeoPackage file, e.g. D:\\data\\gis.gpkg", Label = "GeoPackage")]
    GeoPackage,

    [PipeEnumValue("A gView feature database on PostgreSQL", Label = "gView FDB (PostgreSQL)")]
    FdbPostgreSql,

    [PipeEnumValue("A gView feature database on SQL Server", Label = "gView FDB (SQL Server)")]
    FdbSqlServer,

    [PipeEnumValue("A gView feature database in a SQLite file", Label = "gView FDB (SQLite)")]
    FdbSqlite,

    [PipeEnumValue("Any other gView dataset plugin, by its plugin GUID", Label = "Other (plugin GUID)")]
    Other
}

[PipeCredentialType(TypeId, "gView Datasource",
    Description = "A data source of gView GIS (PostGIS, SDE, shape files, …) for Read Features and Write Features")]
public sealed class GisDatasourceCredential
{
    public const string TypeId = "4f3b8e52-9c1d-4a7e-b6f0-2d5a9e8c1b37";

    [PipeParameter(Label = "Type", Order = 1, Description = "Kind of data source, e.g. PostGIS")]
    public GisDatasourceType Type { get; set; } = GisDatasourceType.PostGis;

    [PipeParameter(Label = "Plugin GUID", Order = 2, ShowWhen = nameof(Type), ShowWhenValues = ["other"],
        Description = "GUID of the gView dataset plugin, e.g. 7a972c29-4955-4899-b142-98f6e05bb063 for OGR")]
    public string? PluginGuid { get; set; }

    [Required, Secret, PipeParameter(Label = "Connection string", Order = 3, Multiline = true,
        Description = "The connection string as gView expects it for this type, e.g. a folder for shape files; may hold a password")]
    public string ConnectionString { get; set; } = "";

    static private readonly Dictionary<GisDatasourceType, Guid> PluginGuids = new()
    {
        [GisDatasourceType.PostGis] = new("206CF40B-D4D9-4e85-B872-D2E63C3556BA"),
        [GisDatasourceType.SqlServerGeometry] = new("69A965B6-E7F6-4C67-A8F4-57AEDF9541C3"),
        [GisDatasourceType.SdeSqlServer] = new("F7394B37-1397-4914-B1D0-5A03B11D2949"),
        [GisDatasourceType.ShapeFiles] = new("80F48262-D412-41fb-BF43-2D611A2ABF42"),
        [GisDatasourceType.GeoPackage] = new("35c6d28f-7c69-4639-b8c9-26e64762da00"),
        [GisDatasourceType.FdbPostgreSql] = new("33254063-133D-4b17-AAE2-46AF7A7DA733"),
        [GisDatasourceType.FdbSqlServer] = new("3B870AB5-8BE0-4a00-911D-ECC6C83DD6B4"),
        [GisDatasourceType.FdbSqlite] = new("36DEB6AC-EA0C-4B37-91F1-B2E397351555")
    };

    /// <summary>The gView plugin GUID of the dataset type.</summary>
    public Guid DatasetPluginGuid()
    {
        if (Type != GisDatasourceType.Other)
        {
            return PluginGuids[Type];
        }

        return Guid.TryParse(PluginGuid, out var guid)
            ? guid
            : throw new Abstraction.PipeException("The credential has no valid plugin GUID");
    }
}
