using gView.Blazor.Core.Extensions;
using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Razor.Dialogs.Models;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("1CA4CC29-FA96-4E0B-862F-C1D8CEDA7335")]
public class SaveDocumentAs : ICartoButton
{
    public string Name => "Save As ...";

    public string ToolTip => "Save the current map under a new filename";

    public string Icon => "basic:disks-white";

    public CartoToolTarget Target => CartoToolTarget.File;

    public int SortOrder => 4;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope) => !scope.Document.Readonly;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var lastAccessedDocuments = await scope.Settings.GetLastAccessedDocuments() ?? [];

        var model = lastAccessedDocuments.Count() == 0 || scope.Document?.Map.IsEmpty() != false
            ? new() // on empty maps dont show this dialog.
            : await scope.ShowModalDialog(
                typeof(Razor.Components.Dialogs.OpenPreviousMapDialog),
                "Override existing map ...",
                new Razor.Components.Dialogs.Models.OpenPreviousMapDialogModel()
                {
                    Items = lastAccessedDocuments
                }
            );

        string? mxlFilePath = model?.Selected switch
        {
            null => await FromCreateFileDialogAsync(scope),
            _ => model.Selected.Path
        };

        if (!String.IsNullOrEmpty(mxlFilePath))
        {
            bool? performEncryption = await PromptPerformEncrypted(scope);

            if (performEncryption.HasValue) // otherweise user has canceled 
            {
                return await scope.SaveCartoDocument(mxlFilePath, performEncryption.Value);
            }
        }

        return false;
    }

    async private Task<string?> FromCreateFileDialogAsync(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowKnownDialog(
                KnownDialogs.ExplorerDialog,
                title: "Save current map",
                model: new ExplorerDialogModel()
                {
                    Filters = new List<ExplorerDialogFilter> {
                        new SaveFileFilter("Map", "*.mxl")
                    },
                    Mode = ExploerDialogMode.Save
                }
             );

        if (model?.Result.ExplorerObjects != null
            && model.Result.ExplorerObjects.Count == 1
            && !String.IsNullOrWhiteSpace(model.Result.Name))
        {
            string fileTitle = model.Result.Name.Trim();
            if (!fileTitle.EndsWith(".mxl", StringComparison.OrdinalIgnoreCase))
            {
                fileTitle = $"{fileTitle}.mxl";
            }

            return Path.Combine(model.Result.ExplorerObjects.First().FullName, fileTitle);

        }

        return null;
    }

    async private Task<bool?> PromptPerformEncrypted(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowKnownDialog(
                KnownDialogs.PromptBoolDialog,
                title: "Encryption",
                model: new PromptDialogModel<bool>
                {
                    Value = true,
                    Prompt = "Encrypt connection strings",
                    HelperText = "If this is not checked, connectionstring are stored in clear text in the mapping file"
                }
            );

        return model switch
        {
            null => null,
            _ => model.Value
        };
    }


}
