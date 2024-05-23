using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Common;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class ModuleSettingsModel : IDialogResultItem
{
    public IEnumerable<IMapApplicationModule>? Modules { get; set; }
}