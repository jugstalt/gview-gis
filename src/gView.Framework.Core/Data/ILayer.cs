using gView.Framework.Core.Common;

namespace gView.Framework.Core.Data
{
    public interface ILayer : IDatasetElement, INamespace
    {
        bool Visible { get; set; }

        double MinimumScale { get; set; }
        double MaximumScale { get; set; }

        double MinimumLabelScale { get; set; }
        double MaximumLabelScale { get; set; }

        double MaximumZoomToFeatureScale { get; set; }

        IGroupLayer GroupLayer { get; }
    }
}