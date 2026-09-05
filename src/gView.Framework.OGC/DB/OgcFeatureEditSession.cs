#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace gView.Framework.OGC.DB
{
    /// <summary>
    /// A transactional edit batch over one <see cref="OgcSpatialDataset"/> (PostGIS, MS SQL
    /// Spatial, SDE, SpatiaLite): every insert/update/delete runs on the same connection and
    /// transaction. <see cref="Commit"/> makes them permanent; disposing without a successful
    /// commit rolls everything back.
    /// </summary>
    internal sealed class OgcFeatureEditSession : IFeatureEditSession
    {
        private readonly OgcSpatialDataset _dataset;
        private readonly DbConnection _connection;
        private readonly DbTransaction _transaction;
        private bool _committed;
        private bool _failed;

        public OgcFeatureEditSession(OgcSpatialDataset dataset, DbConnection connection, DbTransaction transaction)
        {
            _dataset = dataset;
            _connection = connection;
            _transaction = transaction;
        }

        public string LastErrorMessage
        {
            get => _dataset.LastErrorMessage;
            set => _dataset.LastErrorMessage = value;
        }

        public Task<bool> Insert(IFeatureClass fClass, List<IFeature> features, bool returnIds = false)
            => Run(_dataset.InsertInternal(fClass, features, returnIds, _connection, _transaction));

        public Task<bool> Update(IFeatureClass fClass, List<IFeature> features)
            => Run(_dataset.UpdateInternal(fClass, features, _connection, _transaction));

        public Task<bool> Delete(IFeatureClass fClass, int oid)
            => Run(_dataset.DeleteInternal(fClass, oid, _connection, _transaction));

        private async Task<bool> Run(Task<bool> operation)
        {
            if (_failed)
            {
                return false;
            }

            bool ok = await operation;
            if (!ok)
            {
                _failed = true;
            }

            return ok;
        }

        public Task<bool> Commit()
        {
            if (_failed || _committed)
            {
                return Task.FromResult(false);
            }

            try
            {
                _transaction.Commit();
                _committed = true;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _dataset.LastErrorMessage = ex.Message;
                return Task.FromResult(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!_committed)
                {
                    try { _transaction.Rollback(); } catch { /* connection may already be gone */ }
                }
            }
            finally
            {
                await _transaction.DisposeAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}
