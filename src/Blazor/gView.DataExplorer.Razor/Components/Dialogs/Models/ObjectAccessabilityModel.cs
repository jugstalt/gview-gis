using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.IO;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.IO;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class ObjectAccessabilityModel : IDialogResultItem
{
    private IExplorerObject? _exObject;

    public required IExplorerObject ExplorerObject
    {
        get => _exObject!;
        set
        {
            _exObject = value;
            Accessability = value is IExplorerObjectAccessability accessability 
                ? accessability.Accessability 
                : ConfigAccessability.Creator;
        }
    }

    public ConfigAccessability Accessability { get; set; }
}
