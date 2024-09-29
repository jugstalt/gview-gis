using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("540E07A0-B783-4D6D-AF65-3BC63740A3E9")]
public class SaveDocument : ICartoButton
{
    public string Name => "Save Map";

    public string ToolTip => "Save the current map";

    public string Icon => "basic:disk-white";

    public CartoToolTarget Target => CartoToolTarget.File;

    public int SortOrder => 3;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
        => !scope.Document.Readonly 
        && !String.IsNullOrEmpty(scope.Document.FilePath);

    public Task<bool> OnClick(ICartoApplicationScopeService scope)
        => File.Exists(scope.Document.FilePath) switch
        {
            true => scope.SaveCartoDocumentAsync(scope.Document.FilePath, true),
            false => Task.FromResult(false)
        };

}
