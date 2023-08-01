namespace gView.DataExplorer.Razor.Components.Dialogs.Models;
public class NewFdbModel : ConnectionStringModel
{
    public NewFdbModel()
        : this("", true)
    {
    }

    public NewFdbModel(string providerId,
                       bool UseProviderInConnectionString)
        : base(providerId, UseProviderInConnectionString)
    {
    }

    public string DatabaseName { get; set; }
    public bool CreateRepositoryOnly { get; set; }
}
