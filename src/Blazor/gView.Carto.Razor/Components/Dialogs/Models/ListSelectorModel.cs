using gView.Blazor.Models.Dialogs;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class ListSelectorModel<TItem> : IDialogResultItem
{
    public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();

    public ResultClass<TItem> Result { get; set; } = new();

    #region Classes

    public class ResultClass<T>
    {
        public T? SelectedItem { get; set; }
    }

    #endregion
}
