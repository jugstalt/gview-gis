using gView.Framework.Core.Data;

namespace gView.Framework.Core.FDB
{
    public interface IFeatureImportEvents
    {
        void BeforeInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc);
        void AfterInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc);

    }
}
