using gView.Carto.Core;
using gView.Carto.Core.Extensions;
using gView.Carto.Core.Reflection;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Models;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("D1464197-2220-4895-954A-23A1379CFDFB")]
[RestorePointAction(RestoreAction.SetRestorePointOnClick)]
internal class TocSettings : ICartoButton
{
    public string Name => "TOC Settings";

    public string ToolTip => "Edit TOC Settings";

    public string Icon => "basic:edit-text";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 8;

    public void Dispose()
    {
    }

    public bool IsVisible(ICartoApplicationScopeService scope)
        => !scope.Document.Readonly; //  scope.Document.Map.TOC.Elements.Count() > 0;

    public bool IsDisabled(ICartoApplicationScopeService scope) => false;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var original = scope.Document.Map as Map;
        var clone = original?.Clone() as Map;

        if (original is null || clone?.TOC is null)
        {
            return false;
        }

        var selectedTocElement = scope.SelectedTocTreeNode?.Value;
        var selectedGroupElement = selectedTocElement is null || selectedTocElement.IsGroupElement()
                                     ? selectedTocElement
                                     : selectedTocElement.ParentGroup;

        var model = await scope.ShowModalDialog(
            typeof(Razor.Components.Dialogs.TocOrderingDialog),
                    "Toc Settings",
                    new TocOrderingModel()
                    {
                        SelectedGroupElement = selectedGroupElement,
                        MapName = original.Name,
                        Toc = original.TOC
                    },
                    new ModalDialogOptions()
                    {
                        Width = ModalDialogWidth.ExtraExtraLarge
                    });

        //if(model?.Toc is not null)
        //{
        //    ((Map)original).TOC = model.Toc;
        //}

        await scope.EventBus.FireMapSettingsChangedAsync();

        return true;
    }
}
