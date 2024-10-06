using gView.Carto.Core;
using gView.Carto.Core.Extensions;
using gView.Carto.Core.Models.ToolEvents;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("FF437512-0CE4-44C1-888A-D1ECE06FED19")]
internal class Identify : ICartoTool
{
    #region ICartoButton

    public string Name => "Identify";

    public string ToolTip => "Identify Geo Object";

    public ToolType ToolType => ToolType.Click | ToolType.BBox;

    public string Icon => "webgis:identify";

    public CartoToolTarget Target => CartoToolTarget.Tools;

    public int SortOrder => 0;

    public void Dispose()
    {

    }

    public bool IsVisible(ICartoApplicationScopeService scope) => true;
    public bool IsDisabled(ICartoApplicationScopeService scope) => false;

    public Task<bool> OnClick(ICartoApplicationScopeService scope)
        => Task.FromResult(true);

    #endregion

    #region ICartoTool

    public Type UIComponent => typeof(gView.Carto.Razor.Components.Tools.Identify);

    public object? ToolContext { get; } = null;

    public void InitializeScope(ICartoApplicationScopeService scope) { }

    public string ToolBoxTitle(ICartoApplicationScopeService scope) => this.Name;

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope, ToolEventArgs mapEvent)
    {
        var queryLayer = scope.SelectedTocTreeNode?.TocElement.CollectQueryableLayers() ?? [];
        if (!queryLayer.Any())
        {
            return false;
        }

        IGeometry? queryGeometry = mapEvent.GetGeometry();

        if (queryGeometry == null)
        {
            return false;
        }

        foreach (var layer in scope.DataTableService.Layers)
        {
            scope.DataTableService.GetProperties(layer).IdentifyFilter = null;

            if (layer is IFeatureHighlighting featureHighlighting)
            {
                featureHighlighting.FeatureHighlightFilter = null;
            }
        }

        foreach (var layer in queryLayer)
        {
            if (layer.Class is IFeatureClass featureClass)
            {
                if (featureClass.SpatialReference is null)
                {
                    continue; //todo: Show the user a warning?
                }

                scope.DataTableService.AddIfNotExists(layer);
                var tableProperties = scope.DataTableService.GetProperties(layer);

                tableProperties.IdentifyFilter = new SpatialFilter()
                {
                    SubFields = "*",
                    FilterSpatialReference = featureClass.SpatialReference,
                    Geometry = queryGeometry.ToProjectedEnvelope(scope, featureClass.SpatialReference)
                };
                tableProperties.DataMode = Blazor.Models.DataTable.Mode.Identify;
            }
        }

        await scope.EventBus.FireShowDataTableAsync(queryLayer.First());

        return true;
    }

    #endregion
}
