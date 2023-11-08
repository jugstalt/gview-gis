using gView.Carto.Core.Abstractions;

namespace gView.Carto.Core.Services.Abstraction;
public interface ICartoApplicationScopeService
{
    public ICartoDocument Document { get; }

    public CartoEventBusService EventBus { get; }
}
