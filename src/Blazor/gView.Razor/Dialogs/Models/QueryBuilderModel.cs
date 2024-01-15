using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;

namespace gView.Razor.Dialogs.Models;

public class QueryBuilderModel : IDialogResultItem
{
    public ITableClass? TableClass { get; set; }
    public bool CanSelect { get; set; }

    public string QueryString { get; set; } = string.Empty;
    public bool Select { get; set; }
}
