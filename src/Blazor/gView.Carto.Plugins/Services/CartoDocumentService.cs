using gView.Carto.Core.Abstractions;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Carto;

namespace gView.Carto.Plugins.Services;
internal class CartoDocumentService : ICartoDocumentService
{
    private readonly ICartoApplicationScopeService _appScope;

    public CartoDocumentService(ICartoApplicationScopeService appScope)
    {
        _appScope = appScope;
    }

    public ICartoDocument Document => _appScope.Document;
    public IMap CurrentMap => Document.Map;
}
