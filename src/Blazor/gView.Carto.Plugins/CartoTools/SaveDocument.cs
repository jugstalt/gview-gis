using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.IO;
using gView.Framework.Common;
using gView.Carto.Core.Services.Abstraction;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("540E07A0-B783-4D6D-AF65-3BC63740A3E9")]
public class SaveDocument : ICartoTool
{
    public string Name => "Save Map";

    public string ToolTip => "Save the current map";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:disk-white";

    public CartoToolTarget Target => CartoToolTarget.File;

    public int SortOrder => 3;

    public void Dispose()
    {
        
    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
    {
        return !String.IsNullOrEmpty(scope.Document.FilePath);
    }

    public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        if (File.Exists(scope.Document.FilePath))
        {
            XmlStream stream = new XmlStream("MapApplication", true);
            stream.Save("MapDocument", scope.Document);

            stream.WriteStream(scope.Document.FilePath);

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
