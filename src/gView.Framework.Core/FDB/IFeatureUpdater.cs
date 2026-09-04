using gView.Framework.Core.Data;
using gView.Framework.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    public interface IFeatureUpdater : IErrorMessage
    {
        Task<bool> Insert(IFeatureClass fClass, IFeature feature);

        /// <summary>
        /// Inserts a list of features.
        /// </summary>
        /// <param name="returnIds">
        /// When <c>true</c>, the database assigned row id is written back to
        /// <see cref="gView.Framework.Core.Data.IOID.OID"/> of each passed feature on success.
        /// This can cost a little more per row (the insert statement has to return the id), so
        /// it defaults to <c>false</c>, which keeps the plain insert path. Not every provider
        /// supports it - unsupported providers simply insert without back-filling the id.
        /// </param>
        Task<bool> Insert(IFeatureClass fClass, List<IFeature> features, bool returnIds = false);

        Task<bool> Update(IFeatureClass fClass, IFeature feature);
        Task<bool> Update(IFeatureClass fClass, List<IFeature> features);

        Task<bool> Delete(IFeatureClass fClass, int oid);
        Task<bool> Delete(IFeatureClass fClass, string where);

        int SuggestedInsertFeatureCountPerTransaction { get; }
    }
}
