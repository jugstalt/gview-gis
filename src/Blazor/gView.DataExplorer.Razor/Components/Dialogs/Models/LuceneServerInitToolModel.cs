using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;
public class LuceneServerInitToolModel : IDialogResultItem
{
    public string JsonFile { get; set; } = "";

    public string LuceneServerUrl { get; set; } = "";
    public string IndexName { get; set; } = "";
    public bool DeleteIndex { get; set; } = true;
    public string PhoneticAlgorithm { get; set; } = LuceneServerNET.Core.Phonetics.Algorithm.None.ToString();

    public IEnumerable<IFeatureClass>? SourceFeatureClasses { get; set; }
}
