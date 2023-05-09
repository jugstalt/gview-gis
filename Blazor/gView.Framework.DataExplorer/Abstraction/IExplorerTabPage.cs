using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerTabPage : IOrder
{
    /*System.Windows.Forms.Control*/ object Control { get; }

    //System.Guid ExplorerObjectGUID { get; }

    void OnCreate(object hook);

    Task<bool> OnShow();
    void OnHide();

    IExplorerObject GetExplorerObject();
    Task SetExplorerObjectAsync(IExplorerObject value);

    Task<bool> ShowWith(IExplorerObject exObject);
    string Title { get; }

    Task<bool> RefreshContents();
}
