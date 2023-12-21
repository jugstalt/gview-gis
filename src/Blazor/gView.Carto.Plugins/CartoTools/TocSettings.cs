using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.Carto;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Cartography;
using gView.Framework.Blazor.Models;
using gView.Carto.Core.Extensions;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("D1464197-2220-4895-954A-23A1379CFDFB")]
internal class TocSettings : ICartoTool
{
    public string Name => "TOC Settings";

    public string ToolTip => "Edit TOC Settings";

    public ToolType ToolType => ToolType.Click;

    public string Icon => "basic:edit-text";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 98;

    public void Dispose()
    { 
    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
        => true; //  scope.Document.Map.TOC.Elements.Count() > 0;

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        var original = scope.Document.Map as Map;
        var clone = original?.Clone() as Map;

        if (original is null || clone?.TOC is null)
        {
            return false;
        }

        var selectedTocElement = scope.SelectedTocTreeNode?.TocElement;
        var selectedGroupElement = selectedTocElement is null || selectedTocElement.IsGroupElement()
                                     ? selectedTocElement
                                     : selectedTocElement.ParentGroup;

        var model = await scope.ShowModalDialog(
            typeof(Razor.Components.Dialogs.TocOrderingDialog),
                    "Toc Ordering",
                    new TocOrderingModel()
                    {
                        SelectedGroupElement = selectedGroupElement,
                        MapName = original.Name,
                        Toc = original.TOC
                    },
                    new ModalDialogOptions(){
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
