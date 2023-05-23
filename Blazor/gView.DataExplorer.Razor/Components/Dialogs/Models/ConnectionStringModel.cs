using gView.Blazor.Models.Dialogs;
using gView.Framework.Db;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class ConnectionStringModel : IDialogResultItem
{
    private readonly string _providerId;
    private readonly DbConnectionString _dbConnectionString;

    public ConnectionStringModel() 
        : this("")
    {
    }

    public ConnectionStringModel(string providerId,
                                 DbConnectionString? dbConnectionString = null)
    {
        _providerId = providerId;
        _dbConnectionString = dbConnectionString ?? new DbConnectionString();
    }

    public string ProviderId => _providerId;

    public DbConnectionString DbConnectionString => _dbConnectionString;
}
