using SpatialNET;
using SpatialNET.Network.Building;

namespace gView.Framework.NetworkModel.Ids;

/// <summary>The reference of a node or edge to the feature it came from.</summary>
/// <param name="FeatureClass">The index of the feature class in the network (see network.json).</param>
/// <param name="Oid">The object id of the feature.</param>
public readonly record struct FeatureRef(int FeatureClass, long Oid)
{
    public override string ToString() => $"{FeatureClass}.{Oid}";
}

/// <summary>
/// The ids of nodes and edges as one long: | 12 bits feature class | 40 bits object id | 11 bits part |.
/// Helper nodes use the feature class 4095 and a hash of their location rounded to the tolerance and of their type
/// (helper nodes of two tiers may share a location), so all ids follow from the data and stay the same between builds.
/// </summary>
public sealed class NetworkIds : INetworkIdFactory<long, long, FeatureRef>
{
    public const int PartBits = 11;
    public const int OidBits = 40;
    public const int FeatureClassBits = 12;
    public const int HelperFeatureClass = (1 << FeatureClassBits) - 1;
    public const int MaxFeatureClass = HelperFeatureClass - 1;
    public const long MaxOid = (1L << OidBits) - 1;
    public const int MaxParts = 1 << PartBits;

    private readonly double _tolerance;

    public NetworkIds(double tolerance) => _tolerance = tolerance;

    public static long Compose(int featureClass, long oid, int part = 0)
    {
        if (featureClass < 0 || featureClass > HelperFeatureClass)
        {
            throw new ArgumentOutOfRangeException(nameof(featureClass));
        }
        if (oid < 0 || oid > MaxOid)
        {
            throw new ArgumentOutOfRangeException(nameof(oid), $"Object id {oid} does not fit into {OidBits} bits.");
        }
        if (part < 0 || part >= MaxParts)
        {
            throw new ArgumentOutOfRangeException(nameof(part), $"A line splits into more than {MaxParts} parts.");
        }
        return ((long)featureClass << (OidBits + PartBits)) | (oid << PartBits) | (long)part;
    }

    public static (int FeatureClass, long Oid, int Part) Decompose(long id)
        => ((int)(id >> (OidBits + PartBits)), (id >> PartBits) & MaxOid, (int)(id & (MaxParts - 1)));

    public static bool IsHelper(long id) => Decompose(id).FeatureClass == HelperFeatureClass;

    public static long NodeId(FeatureRef source) => Compose(source.FeatureClass, source.Oid);

    private readonly HashSet<long> _helperIds = [];

    public long NewHelperNode(Coordinate location, int typeId)
    {
        // the location rounded to the tolerance grid and the type, hashed into the 51 free bits; two helper nodes may round
        // to the same cell (a chain of ends wider than the tolerance), then the next salt is taken
        long ix = (long)Math.Round(location.X / _tolerance);
        long iy = (long)Math.Round(location.Y / _tolerance);
        for (long salt = 0; ; salt++)
        {
            long id = HelperId(ix, iy, typeId, salt);
            if (_helperIds.Add(id))
            {
                return id;
            }
        }
    }

    private static long HelperId(long ix, long iy, int typeId, long salt)
    {
        // FNV-1a over both cell indices, the type (only when set, so untyped helper nodes keep their ids) and the salt
        ulong hash = 14695981039346656037UL;
        foreach (var value in typeId == 0 ? new[] { ix, iy, salt } : new[] { ix, iy, typeId, salt })
        {
            ulong v = (ulong)value;
            for (int i = 0; i < 8; i++)
            {
                hash ^= (v >> (i * 8)) & 0xff;
                hash *= 1099511628211UL;
            }
        }
        long low = (long)(hash & ((1UL << (OidBits + PartBits)) - 1));
        return ((long)HelperFeatureClass << (OidBits + PartBits)) | low;
    }

    public long NewEdge(FeatureRef source, int part, int partCount) => Compose(source.FeatureClass, source.Oid, part);
}
