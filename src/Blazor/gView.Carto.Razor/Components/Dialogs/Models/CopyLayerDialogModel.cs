using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public enum CopyLayerMode
{
    Copy,
    SplitByFilter
}

public class CopyLayerDialogModel : IDialogResultItem
{
    public CopyLayerMode CopyMode { get; set; } = CopyLayerMode.Copy;

    public IMap? Map { get; set; }
    public ILayer? Layer { get; set; }

    public string? FilterField { get; set; }
    public string? NewNamePattern { get; set; }
}


