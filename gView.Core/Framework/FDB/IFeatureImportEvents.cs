using gView.Framework.Data;

namespace gView.Framework.FDB
{
    public interface IFeatureImportEvents
    {
        void BeforeInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc);
        void AfterInsertFeaturesEvent(IFeatureClass sourceFc, IFeatureClass destFc);

    }
}
