namespace gView.Framework.Core.Data
{
    public interface IFeatureLayerComposition
    {
        FeatureLayerCompositionMode CompositionMode { get; set; }
        float CompositionModeCopyTransparency { get; set; }
    }
}
