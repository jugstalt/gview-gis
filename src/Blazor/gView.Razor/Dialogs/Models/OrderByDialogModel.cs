using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;

namespace gView.Razor.Dialogs.Models;
public class OrderByDialogModel : IDialogResultItem
{
    public object? Instance { get; set; }

    public string OrderByClause { get; set; } = string.Empty;
}
