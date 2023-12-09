using gView.Carto.Core.Abstractions;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;

namespace gView.Carto.Plugins.Services;
internal class CartoDocumentService : ICartoDocumentService
{
    private readonly ICartoDocument _document;

    public CartoDocumentService(IApplicationScope appScope)
    {
        _document = appScope.ToCartoScopeService().Document;
    }

    public ICartoDocument Document => _document;
}
