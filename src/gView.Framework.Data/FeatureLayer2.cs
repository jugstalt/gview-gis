using gView.Framework.Core.Data;

namespace gView.Framework.Data
{
    public class FeatureLayer2 : FeatureSelection
    {
        public FeatureLayer2()
        {
        }
        public FeatureLayer2(IFeatureClass featureClass)
            : base(featureClass)
        {
        }
        public FeatureLayer2(IFeatureLayer layer)
            : base(layer)
        {
        }
    }
}
