using gView.Framework.Data;
using gView.Framework.system;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.FDB
{
    public interface IFeatureUpdater : IErrorMessage
    {
        Task<bool> Insert(IFeatureClass fClass, IFeature feature);
        Task<bool> Insert(IFeatureClass fClass, List<IFeature> features);

        Task<bool> Update(IFeatureClass fClass, IFeature feature);
        Task<bool> Update(IFeatureClass fClass, List<IFeature> features);

        Task<bool> Delete(IFeatureClass fClass, int oid);
        Task<bool> Delete(IFeatureClass fClass, string where);

        int SuggestedInsertFeatureCountPerTransaction { get; }
    }
}
