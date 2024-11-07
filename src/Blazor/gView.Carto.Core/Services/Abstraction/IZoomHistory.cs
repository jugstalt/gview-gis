using gView.Framework.Core.Geometry;

namespace gView.Carto.Core.Services.Abstraction;

public interface IZoomHistory
{
    void Push(IEnvelope bounds);

    IEnvelope? Pop();

    void Clear();

    bool SuppressOnce { get; set; }

    bool HasItems { get; }
}