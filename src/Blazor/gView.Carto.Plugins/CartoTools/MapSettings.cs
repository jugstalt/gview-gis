using gView.Carto.Core;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Cartography;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("20BB9506-D6AE-4A81-AC2D-733DEE4465A4")]
internal class MapSettings : ICartoTool
{
    public string Name => "Map Settings";

    public string ToolTip => "";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:settings";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 99;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope) => true;

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        var original = scope.Document.Map as Map;
        var clone = original?.Clone() as Map;

        if (original is null || clone is null)
        {
            return false;
        }

        clone.ZoomTo(original.Display.Envelope);
        clone.Display.ImageWidth = original.Display.ImageWidth;
        clone.Display.ImageHeight = original.Display.ImageHeight;

        var model = await scope.ShowModalDialog(typeof(gView.Carto.Razor.Components.Dialogs.MapSettingsDialog),
                                                    "Map Settings",
                                                    new Razor.Components.Dialogs.Models.MapSettingsModel()
                                                    {
                                                        Map = clone
                                                    });

        if (model?.Map != null)
        {
            #region General

            if (original.Name?.Equals(clone.Name) == false
                && !string.IsNullOrEmpty(clone.Name = clone.Name?.Trim()))
            {
                original.Name = clone.Name;

                await scope.EventBus.FireRefreshContentTreeAsync();
            }
            original.ReferenceScale = clone.ReferenceScale;
            original.Display.MapUnits = clone.Display.MapUnits;
            original.Display.DisplayUnits = clone.Display.DisplayUnits;
            original.Display.BackgroundColor = clone.Display.BackgroundColor;

            #endregion

            #region Spatial Reference

            original.SpatialReference = clone.SpatialReference;
            original.LayerDefaultSpatialReference = clone.LayerDefaultSpatialReference;

            #endregion

            #region Description

            original.Title = clone.Title;
            original.SetLayerDescription(Map.MapDescriptionId, clone.GetLayerDescription(Map.MapDescriptionId));
            original.SetLayerCopyrightText(Map.MapCopyrightTextId, clone.GetLayerCopyrightText(Map.MapCopyrightTextId));

            #endregion

            #region Resources

            // clone and original shares resource container
            // changes are live (no cancel!)

            #endregion

            await scope.EventBus.FireRefreshMapAsync();
        }

        return true;
    }
}
