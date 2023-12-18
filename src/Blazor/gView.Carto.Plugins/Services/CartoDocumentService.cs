using gView.Carto.Core.Abstractions;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Carto;

namespace gView.Carto.Plugins.Services;
internal class CartoDocumentService : ICartoDocumentService
{
    private readonly ICartoDocument _document;

    public CartoDocumentService(ICartoApplicationScopeService appScope)
    {
        _document = appScope.Document;
    }

    public ICartoDocument Document => _document;
    public IMap CurrentMap => Document.Map;
}
