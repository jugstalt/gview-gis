using gView.Blazor.Models.Content;
using gView.Framework.UI;
using System;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerTabPage : IOrder
{
    Type RazorComponent { get; }

    void OnCreate(object hook);

    Task<bool> OnShow();
    void OnHide();

    IExplorerObject GetExplorerObject();
    Task SetExplorerObjectAsync(IExplorerObject value);

    Task<bool> ShowWith(IExplorerObject exObject);
    string Title { get; }

    Task<ContentItemResult> RefreshContents();
}
