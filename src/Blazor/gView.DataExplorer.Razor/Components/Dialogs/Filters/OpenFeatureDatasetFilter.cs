using gView.Framework.Core.Data;

namespace gView.DataExplorer.Razor.Components.Dialogs.Filters;

public class OpenFeatureDatasetFilter : ExplorerOpenDialogFilter
{
    public OpenFeatureDatasetFilter() : base("Feature Dataset")
    {
        this.ObjectTypes.Add(typeof(IFeatureDataset));
    }
}
