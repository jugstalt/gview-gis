using gView.Framework.Core.Carto;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Data
{
    public interface IFeatureLayer : ILayer
    {
        IFeatureRenderer FeatureRenderer { get; set; }
        IFeatureRenderer SelectionRenderer { get; set; }
        ILabelRenderer LabelRenderer { get; set; }

        bool ApplyRefScale { get; set; }
        bool ApplyLabelRefScale { get; set; }

        float MaxRefScaleFactor { get; set; }
        float MaxLabelRefScaleFactor { get; set; }

        IFeatureClass FeatureClass { get; }
        IQueryFilter FilterQuery { get; set; }

        IFieldCollection Fields { get; }

        FeatureLayerJoins Joins { get; set; }

        GeometryType LayerGeometryType { get; set; }
    }
}