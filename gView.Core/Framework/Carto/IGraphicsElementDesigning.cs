using gView.Framework.Geometry;

namespace gView.Framework.Carto
{
    public interface IGraphicsElementDesigning
    {
        //bool Selected { get; set; }
        IGraphicElement2 Ghost { get; }
        IHitPositions HitTest(IDisplay display, IPoint point);
        void Design(IDisplay display, IHitPositions hit, double dx, double dy);
        bool TrySelect(IDisplay display, IEnvelope envelope);
        bool TrySelect(IDisplay display, IPoint point);

        bool RemoveVertex(IDisplay display, int index);
        bool AddVertex(IDisplay display, IPoint point);
    }
}