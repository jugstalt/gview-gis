using gView.Carto.Core;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Data;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("805D37BF-94CB-45AB-838B-7087036CAB0C")]
internal class DataTable : ICartoTool
{
    public string Name => "Data Table";

    public string ToolTip => "Show Data Table";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:table";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 10;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
        => scope.SelectedTocTreeNode?
                 .TocElement?
                 .Layers?.Count == 1
        && scope.SelectedTocTreeNode
                .TocElement
                .Layers.First()
                .Class is ITableClass;

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        var layer = scope.SelectedTocTreeNode?
                .TocElement?
                .Layers?.FirstOrDefault();

        IDSelectionSet idSelectionSet = new();
        IFeatureSelection? featureSelection = layer as IFeatureSelection;
        if (featureSelection is not null)
        {
            idSelectionSet.Combine(featureSelection.SelectionSet, CombinationMethod.New);
        }

        var tableClass = layer?.Class as ITableClass;

        if (tableClass is null)
        {
            return false;
        }

        //var model = await scope.ShowModalDialog(
        //    typeof(gView.Carto.Razor.Components.Dialogs.DataTableDialog),
        //    tableClass.Name,
        //    new DataTableModel()
        //    {
        //        Layer = layer
        //    },
        //    modalDialogOptions: new ModalDialogOptions()
        //    {
        //        Width = ModalDialogWidth.ExtraExtraLarge,
        //        FullWidth = true,
        //    });

        //if (featureSelection is not null &&
        //    idSelectionSet.IsNotEqual(featureSelection.SelectionSet))
        //{
        //    await scope.EventBus.FireRefreshMapAsync(DrawPhase.Selection);
        //}

        await scope.EventBus.FireShowDataTable(layer!);

        return true;
    }
}
