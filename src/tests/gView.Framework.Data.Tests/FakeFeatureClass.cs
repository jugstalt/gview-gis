using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;

namespace gView.Framework.Data.Tests;

/// <summary>
/// Feature class with fields only, for tests of request checks against the table schema.
/// </summary>
internal class FakeFeatureClass : IFeatureClass
{
    private readonly FieldCollection _fields = new();

    public FakeFeatureClass(params (string name, FieldType type)[] fields)
    {
        foreach (var (name, type) in fields)
        {
            _fields.Add(new Field(name, type));
        }
    }

    public static FakeFeatureClass Default() => new(
        ("OBJECTID", FieldType.ID),
        ("NAME", FieldType.String),
        ("POPULATION", FieldType.integer),
        ("AREA_KM2", FieldType.Double),
        ("CREATED", FieldType.Date),
        ("ACTIVE", FieldType.boolean),
        ("Straße", FieldType.String),
        ("Feld A", FieldType.String),
        ("SHAPE", FieldType.Shape),
        ("SHAPE.STArea()", FieldType.Double));

    public string Name => "fake";
    public string Aliasname => "fake";
    public IDataset Dataset => null!;
    public IFieldCollection Fields => _fields;
    public IField FindField(string name) => _fields.FindField(name);
    public string IDFieldName => "OBJECTID";
    public string ShapeFieldName => "SHAPE";
    public IEnvelope Envelope => null!;
    public bool HasZ => false;
    public bool HasM => false;
    public ISpatialReference SpatialReference => null!;
    public GeometryType GeometryType => GeometryType.Polygon;

    public Task<ICursor> Search(IQueryFilter filter) => throw new NotSupportedException();
    public Task<ISelectionSet> Select(IQueryFilter filter) => throw new NotSupportedException();
    public Task<int> CountFeatures() => throw new NotSupportedException();
    public Task<IFeatureCursor> GetFeatures(IQueryFilter filter) => throw new NotSupportedException();
}
