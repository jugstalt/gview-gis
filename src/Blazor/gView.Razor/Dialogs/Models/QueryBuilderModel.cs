using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;

namespace gView.Razor.Dialogs.Models;

[Flags]
public enum QueryBuilderAction
{
    None = 0,
    Query = 1,
    Select = 2,
    QueryAndSelect = 4
}

public class QueryBuilderModel : IDialogResultItem
{
    public ITableClass? TableClass { get; set; }
    public QueryBuilderAction Actions { get; set; } = QueryBuilderAction.Query;

    public string QueryString { get; set; } = string.Empty;
    public QueryBuilderAction SelectedAction { get; set; }
}
