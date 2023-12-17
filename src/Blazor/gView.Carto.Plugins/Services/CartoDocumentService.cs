using gView.Carto.Core.Abstractions;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Carto;
using OSGeo_v1.GDAL;
using System.Reflection.Metadata;

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
