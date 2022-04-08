namespace gView.Framework.Network
{
    public interface IGraphEdge
    {
        int Eid { get; }
        int FcId { get; }
        int Oid { get; }

        int N1 { get; }
        int N2 { get; }
    }
}
