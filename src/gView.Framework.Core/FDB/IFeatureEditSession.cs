#nullable enable

using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    /// <summary>
    /// Implemented by an <see cref="IFeatureUpdater"/> that can apply a batch of inserts,
    /// updates and deletes inside a single database transaction (all-or-nothing).
    /// </summary>
    public interface ISupportsFeatureEditSession
    {
        /// <summary>
        /// Opens a transactional edit session. Returns <c>null</c> when the provider cannot
        /// guarantee a transaction spanning several operations - the caller then falls back to
        /// applying the edits one operation at a time.
        /// </summary>
        Task<IFeatureEditSession?> BeginEditSession();
    }

    /// <summary>
    /// A unit of work over one <see cref="IFeatureUpdater"/>: every insert/update/delete runs
    /// on the same connection and transaction. Call <see cref="Commit"/> to make the edits
    /// permanent; disposing without a successful commit rolls everything back.
    /// </summary>
    public interface IFeatureEditSession : IAsyncDisposable, IErrorMessage
    {
        Task<bool> Insert(IFeatureClass fClass, List<IFeature> features, bool returnIds = false);

        Task<bool> Update(IFeatureClass fClass, List<IFeature> features);

        Task<bool> Delete(IFeatureClass fClass, int oid);

        /// <summary>
        /// Commits every edit applied through this session. After a failed commit the session
        /// is rolled back and must be disposed.
        /// </summary>
        Task<bool> Commit();
    }
}
