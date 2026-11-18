using System.Text;

namespace gView.Framework.NetworkModel.Store;

/// <summary>A point of the spatial index: a node, or a merged point (alias) at its own location standing for its node.</summary>
/// <param name="Id">The node id, or the alias id for a merged point.</param>
/// <param name="Node">The node the point resolves to (the same as <paramref name="Id"/> for a node).</param>
public readonly record struct SpatialEntry(long Id, long Node, double X, double Y);

/// <summary>A point found near a location.</summary>
public readonly record struct SpatialHit(SpatialEntry Entry, double Distance);

/// <summary>
/// spatial.idx: every node (real and helper) and every merged point at its location, in a fixed grid, so the nearest
/// node to a coordinate can be found without loading shards (picking edge ends, @x,y references).
/// "SNSP" | version int32 | cell size double | origin x double | origin y double | count int32 |
/// cells int64[] | ids int64[] | nodes int64[] | xs double[] | ys double[], sorted by cell.
/// </summary>
public sealed class SpatialIndex
{
    public const string FileName = "spatial.idx";
    private const int Version = 1;
    private static readonly byte[] Magic = "SNSP"u8.ToArray();

    /// <summary>The cell size is chosen so a cell holds about this many points on average.</summary>
    private const int PointsPerCell = 32;

    private readonly double _cellSize, _originX, _originY;
    private readonly long[] _cells, _ids, _nodes;
    private readonly double[] _xs, _ys;

    private SpatialIndex(double cellSize, double originX, double originY, long[] cells, long[] ids, long[] nodes, double[] xs, double[] ys)
    {
        (_cellSize, _originX, _originY) = (cellSize, originX, originY);
        (_cells, _ids, _nodes, _xs, _ys) = (cells, ids, nodes, xs, ys);
    }

    public int Count => _ids.Length;

    public double CellSize => _cellSize;

    /// <summary>The points within <paramref name="radius"/> of x/y, nearest first, at most <paramref name="count"/>.</summary>
    public IReadOnlyList<SpatialHit> Nearest(double x, double y, double radius, int count = 1)
    {
        var hits = new List<SpatialHit>();
        if (_ids.Length == 0 || count <= 0 || !(radius >= 0) || double.IsNaN(x) || double.IsNaN(y))
        {
            return hits;
        }

        long ix0 = CellIndex(x - radius, _originX), ix1 = CellIndex(x + radius, _originX);
        long iy0 = CellIndex(y - radius, _originY), iy1 = CellIndex(y + radius, _originY);
        if ((ix1 - ix0 + 1) * (iy1 - iy0 + 1) > 1_000_000)
        {
            throw new ArgumentException($"The search radius {radius} is too large for this network (cell size {_cellSize:0.###}).");
        }
        for (long ix = ix0; ix <= ix1; ix++)
        {
            for (long iy = iy0; iy <= iy1; iy++)
            {
                long cell = CellKey(ix, iy);
                int i = Array.BinarySearch(_cells, cell);
                if (i < 0)
                {
                    continue;
                }
                while (i > 0 && _cells[i - 1] == cell) i--;
                for (; i < _cells.Length && _cells[i] == cell; i++)
                {
                    double d = Math.Sqrt((_xs[i] - x) * (_xs[i] - x) + (_ys[i] - y) * (_ys[i] - y));
                    if (d <= radius)
                    {
                        hits.Add(new SpatialHit(new SpatialEntry(_ids[i], _nodes[i], _xs[i], _ys[i]), d));
                    }
                }
            }
        }
        // a node stands first for itself, before the merged points on it
        return hits.OrderBy(h => h.Distance).ThenBy(h => h.Entry.Id == h.Entry.Node ? 0 : 1).ThenBy(h => h.Entry.Id).Take(count).ToList();
    }

    /// <summary>The points within the box (border included), in no particular order.</summary>
    public IEnumerable<SpatialEntry> Within(double minX, double minY, double maxX, double maxY)
    {
        if (_ids.Length == 0 || !(maxX >= minX) || !(maxY >= minY))
        {
            yield break;
        }
        long ix0 = CellIndex(minX, _originX), ix1 = CellIndex(maxX, _originX);
        long iy0 = CellIndex(minY, _originY), iy1 = CellIndex(maxY, _originY);
        double cells = (double)(ix1 - ix0 + 1) * (iy1 - iy0 + 1);
        if (cells > _ids.Length)
        {
            // a box over (nearly) the whole network: going through all points is faster than through the cells
            for (int i = 0; i < _ids.Length; i++)
            {
                if (_xs[i] >= minX && _xs[i] <= maxX && _ys[i] >= minY && _ys[i] <= maxY)
                {
                    yield return new SpatialEntry(_ids[i], _nodes[i], _xs[i], _ys[i]);
                }
            }
            yield break;
        }
        for (long ix = ix0; ix <= ix1; ix++)
        {
            for (long iy = iy0; iy <= iy1; iy++)
            {
                long cell = CellKey(ix, iy);
                int i = Array.BinarySearch(_cells, cell);
                if (i < 0)
                {
                    continue;
                }
                while (i > 0 && _cells[i - 1] == cell) i--;
                for (; i < _cells.Length && _cells[i] == cell; i++)
                {
                    if (_xs[i] >= minX && _xs[i] <= maxX && _ys[i] >= minY && _ys[i] <= maxY)
                    {
                        yield return new SpatialEntry(_ids[i], _nodes[i], _xs[i], _ys[i]);
                    }
                }
            }
        }
    }

    private long CellIndex(double value, double origin) => (long)Math.Floor((value - origin) / _cellSize);

    // 32 bits per axis: a network is never wider than 4 billion cells
    private static long CellKey(long ix, long iy) => (ix << 32) | (iy & 0xffffffffL);

    public static void Write(string path, IReadOnlyCollection<SpatialEntry> entries, double tolerance)
    {
        var points = entries.Where(e => !double.IsNaN(e.X) && !double.IsNaN(e.Y)).ToArray();
        double minX = 0, minY = 0, cellSize = Math.Max(tolerance, 1e-9) * 10;
        if (points.Length > 0)
        {
            minX = points.Min(p => p.X);
            minY = points.Min(p => p.Y);
            double width = points.Max(p => p.X) - minX, height = points.Max(p => p.Y) - minY;
            double area = Math.Max(width, cellSize) * Math.Max(height, cellSize);
            cellSize = Math.Max(cellSize, Math.Sqrt(area * PointsPerCell / points.Length));
        }

        var index = new SpatialIndex(cellSize, minX, minY, [], [], [], [], []);
        var sorted = points
            .Select(p => (Cell: CellKey(index.CellIndex(p.X, minX), index.CellIndex(p.Y, minY)), Point: p))
            .OrderBy(p => p.Cell).ThenBy(p => p.Point.Id)
            .ToArray();

        using var w = new BinaryWriter(File.Create(path), Encoding.UTF8);
        w.Write(Magic);
        w.Write(Version);
        w.Write(cellSize);
        w.Write(minX);
        w.Write(minY);
        w.Write(sorted.Length);
        foreach (var p in sorted) w.Write(p.Cell);
        foreach (var p in sorted) w.Write(p.Point.Id);
        foreach (var p in sorted) w.Write(p.Point.Node);
        foreach (var p in sorted) w.Write(p.Point.X);
        foreach (var p in sorted) w.Write(p.Point.Y);
    }

    public static SpatialIndex Read(string path)
    {
        using var r = new BinaryReader(new MemoryStream(File.ReadAllBytes(path)), Encoding.UTF8);
        if (!r.ReadBytes(4).SequenceEqual(Magic))
        {
            throw new InvalidDataException("Not a network spatial index.");
        }
        int version = r.ReadInt32();
        if (version != Version)
        {
            throw new InvalidDataException($"Unsupported spatial index version {version}.");
        }
        double cellSize = r.ReadDouble(), originX = r.ReadDouble(), originY = r.ReadDouble();
        int count = r.ReadInt32();
        var cells = new long[count];
        var ids = new long[count];
        var nodes = new long[count];
        var xs = new double[count];
        var ys = new double[count];
        for (int i = 0; i < count; i++) cells[i] = r.ReadInt64();
        for (int i = 0; i < count; i++) ids[i] = r.ReadInt64();
        for (int i = 0; i < count; i++) nodes[i] = r.ReadInt64();
        for (int i = 0; i < count; i++) xs[i] = r.ReadDouble();
        for (int i = 0; i < count; i++) ys[i] = r.ReadDouble();
        return new SpatialIndex(cellSize, originX, originY, cells, ids, nodes, xs, ys);
    }
}
