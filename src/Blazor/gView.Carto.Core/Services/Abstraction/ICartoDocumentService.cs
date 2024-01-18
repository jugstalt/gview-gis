using gView.Carto.Core.Abstraction;
using gView.Framework.Core.Carto;

namespace gView.Carto.Core.Services.Abstraction;
public interface ICartoDocumentService
{
    public ICartoDocument Document { get; }
    public IMap CurrentMap { get; }
}
