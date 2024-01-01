using gView.Blazor.Core.Extensions;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Data;

namespace gView.Carto.Plugins.CartoTools
{
    [RegisterPlugIn("EB3EDA0D-1B27-4314-B1DE-915E27B27982")]
    internal class LayerSettings : ICartoTool
    {
        public string Name => "Layer Settings";

        public string ToolTip => "";

        public ToolType ToolType => ToolType.Click;

        public string Icon => "basic:settings";

        public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

        public int SortOrder => 99;

        public void Dispose()
        {

        }

        public bool IsEnabled(ICartoApplicationScopeService scope)
            => scope.SelectedTocTreeNode is not null;

        async public Task<bool> OnEvent(ICartoApplicationScopeService scope)
        {
            await scope.EventBus.FireCloseTocInlineEditorsAsync();

            var originalMap = scope.Document.Map as Map;
            var clonedMap = originalMap?.Clone() as Map;

            if (originalMap is null || clonedMap is null)
            {
                return false;
            }

            var originalLayer = scope.SelectedTocTreeNode?.TocElement?.Layers?.FirstOrDefault() as Layer;
            var clonedLayer = originalLayer?.PersistedClone();

            if (originalLayer is null || clonedLayer is null)  // todo: clone layer?
            {
                return false;
            }

            clonedLayer.Class = originalLayer.Class;

            var tocElement = originalMap.TOC.GetTocElementByLayerId(originalLayer.ID);

            var model = await scope.ShowModalDialog(typeof(gView.Carto.Razor.Components.Dialogs.LayerSettingsDialog),
                                                                $"Layer: {tocElement?.Name}",
                                                                new Razor.Components.Dialogs.Models.LayerSettingsModel()
                                                                {
                                                                    Map = clonedMap,
                                                                    Layer = clonedLayer
                                                                },
                                                                new ModalDialogOptions()
                                                                {
                                                                    Width = ModalDialogWidth.ExtraExtraLarge,
                                                                    FullWidth = false
                                                                });

            if (model is null)
            {
                return false;
            }

            // Copy Selection
            if(clonedLayer is IFeatureSelection clonedSelection 
               && originalLayer is IFeatureSelection originalSelection)
            {
                clonedSelection.SelectionSet = originalSelection.SelectionSet;
            }
            originalMap.ReplaceLayer(originalLayer, clonedLayer);

            #region Description

            originalMap.SetLayerDescription(originalLayer.ID, clonedMap.GetLayerDescription(originalLayer.ID));
            originalMap.SetLayerCopyrightText(originalLayer.ID, clonedMap.GetLayerCopyrightText(originalLayer.ID));

            #endregion

            await scope.EventBus.FireMapSettingsChangedAsync();

            return true;
        }
    }
}
