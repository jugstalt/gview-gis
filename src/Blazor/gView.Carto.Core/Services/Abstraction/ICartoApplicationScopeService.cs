using gView.Blazor.Core.Services;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Models.Tree;
using gView.Framework.Blazor.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.Carto.Core.Services.Abstraction;
public interface ICartoApplicationScopeService : IApplicationScope
{
    ICartoDocument Document { get; }

    ICartoInteractiveToolService Tools { get; }

    CartoEventBusService EventBus { get; }

    CartoDisplayService DisplayService { get; }

    CartoDataTableService DataTableService { get; }

    GeoTransformerService GeoTransformer { get; }

    SettingsService Settings { get; }

    TocTreeNode? SelectedTocTreeNode { get; }
    Task SetSelectedTocTreeNode(TocTreeNode? selectedTocTreeNode);

    Task<bool> LoadCartoDocument(string mxlFilePath);
    Task<bool> SaveCartoDocument(string xmlFilePath, bool performEncryption);
}
