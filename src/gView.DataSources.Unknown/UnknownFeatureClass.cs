using gView.Framework.Core.Data;
using gView.Framework.Data;

namespace gView.DataSources.Unknown;

/// <summary>
/// A placeholder feature class that can be assigned to imported layers whose
/// actual data source is not known yet (e.g. layers imported from an APRX file).
/// All instances share a single <see cref="UnknownFeatureDataset"/> so that the
/// map's dataset list stays lean.
/// </summary>
public class UnknownFeatureClass : FeatureClass
{
    private readonly IDataset _dadtaset;
    public UnknownFeatureClass(IDataset dataset, string name)
    {
        Name = name;
        GeometryType = Framework.Core.Geometry.GeometryType.Aggregate;

        _dadtaset = dataset;
    }

    public override IDataset Dataset => _dadtaset;
}
