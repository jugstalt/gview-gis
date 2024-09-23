using gView.Blazor.Core.Extensions;
using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Blazor;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.GraphicsEngine;
using gView.Razor.Dialogs.Models;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("7A020F6D-9744-4355-AC18-8B31723064A4")]
internal class CopyLayer : ICartoButton
{
    public string ToolTip => "";

    public string Icon => "basic:copy";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 98;

    public string Name => "Copy Layer";

    public bool IsEnabled(ICartoApplicationScopeService scope)
        => scope.SelectedTocTreeNode is not null;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var map = scope.Document.Map as Map;
        var originalLayer = scope.SelectedTocTreeNode?
                                 .TocElement?
                                 .Layers?.FirstOrDefault() as Layer;

        if (map is null || originalLayer is null)
            return false;

        var promptModel = await scope.ShowKnownDialog(
               KnownDialogs.PromptBoolDialog,
               title: $"Copy Layer",
               model: new PromptDialogModel<bool>
               {
                   Value = true,
                   Prompt = $"Copy Layer {originalLayer.TocNameOrLayerTitle(map)}",
                   HelperText = ""
               }
           );

        if (promptModel?.Value != true) return false;

        var newLayers = originalLayer.CreateCopy(map);

        foreach (var newLayer in newLayers)
        {
            if(newLayers.IndexOf(newLayer) == 0)  // first shoud be positioned ater original
            {
                int pos = -1;
                if (map.TOC?.Elements != null)
                {
                    var originalTocElement = scope.Document.Map.TOC.Elements.FirstOrDefault(e => e.Layers != null && e.Layers.Contains(originalLayer));
                    pos = scope.Document.Map.TOC.Elements.IndexOf(originalTocElement) + 1;
                }
                map.AddLayer(newLayer, pos);
            } 
            else
            {
                map.AddLayer(newLayer);
            }
        }

        await scope.EventBus.FireMapSettingsChangedAsync();

        return true;
    }
}