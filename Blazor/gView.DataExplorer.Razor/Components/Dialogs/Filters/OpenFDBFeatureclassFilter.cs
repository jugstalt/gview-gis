namespace gView.DataExplorer.Razor.Components.Dialogs.Filters;

public class OpenFDBFeatureclassFilter : ExplorerOpenDialogFilter
{
    public OpenFDBFeatureclassFilter()
        : base("Shapefile")
    {
        this.ExplorerObjectGUIDs.Add(new Guid("FE6E1EA7-1300-400c-8674-68465859E991"));
        this.ExplorerObjectGUIDs.Add(new Guid("A610B342-E911-4c52-8E35-72A69B52440A"));
    }
}
