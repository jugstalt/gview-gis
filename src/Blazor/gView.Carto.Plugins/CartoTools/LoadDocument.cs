using gView.Blazor.Core.Extensions;
using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("891D1698-1D8C-4785-9197-D8EFD8492C23")]
public class LoadDocument : ICartoInitialButton
{
    public string Name => "Open Map";

    public string ToolTip => "Open an existing map document";

    public string Icon => "basic:folder-white";

    public CartoToolTarget Target => CartoToolTarget.File;

    public int SortOrder => 2;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
    {
        return true;
    }

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var lastAccessedDocuments = await scope.Settings.GetLastAccessedDocuments() ?? [];

        var model = lastAccessedDocuments.Count() == 0 || scope.Document?.Map.IsEmpty() != false
            ? new() // on empty maps dont show this dialog. User see previous maps on the initial tools panel
            : await scope.ShowModalDialog(
                typeof(Razor.Components.Dialogs.OpenPreviousMapDialog),
                "Previous opened maps ...",
                new Razor.Components.Dialogs.Models.OpenPreviousMapDialogModel()
                {
                    Items = lastAccessedDocuments
                }
            );

        if (model is null)
        {
            return false;
        }

        string? mxlFilePath = model.Selected switch
        {
            null => await FromOpenFileDialogAsync(scope),
            _ => model.Selected.Path
        };

        return !String.IsNullOrEmpty(mxlFilePath) switch
        {
            false => false,
            true => await scope.LoadCartoDocumentAsync(mxlFilePath)
        };
    }

    async private Task<string?> FromOpenFileDialogAsync(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowKnownDialog(
                    KnownDialogs.ExplorerDialog,
                    title: "Load existing map",
                    model: new ExplorerDialogModel()
                    {
                        Filters = new List<ExplorerDialogFilter> {
                            new OpenFileFilter("Map", "*.mxl")
                        },
                        Mode = ExploerDialogMode.Open
                    }
                );

        return model?.Result.ExplorerObjects.FirstOrDefault()?.FullName;
    }
}
