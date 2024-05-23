using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Geometry;
using gView.Framework.Db;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;
public class EventTableConnectionModel : IDialogResultItem
{
    public DbConnectionString ConnectionString { get; set; } = new DbConnectionString();

    public string TableName { get; set; } = string.Empty;
    public string IdFieldName { get; set; } = string.Empty;
    public string XFieldName { get; set; } = string.Empty; 
    public string YFieldName { get; set; }= string.Empty;   

    public ISpatialReference? SpatialReference { get; set; } 
}
