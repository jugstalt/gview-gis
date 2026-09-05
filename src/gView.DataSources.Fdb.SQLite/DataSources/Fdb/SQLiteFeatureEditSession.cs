#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.SQLite
{
    /// <summary>
    /// A transactional edit batch over one <see cref="SQLiteFDB"/>: every insert/update/delete
    /// runs on the same connection and transaction. <see cref="Commit"/> makes them permanent;
    /// disposing without a successful commit rolls everything back.
    /// </summary>
    internal sealed class SQLiteFeatureEditSession : IFeatureEditSession
    {
        private readonly SQLiteFDB _fdb;
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;
        private bool _committed;
        private bool _failed;

        private SQLiteFeatureEditSession(SQLiteFDB fdb, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            _fdb = fdb;
            _connection = connection;
            _transaction = transaction;
        }

        public static async Task<SQLiteFeatureEditSession> CreateAsync(SQLiteFDB fdb, string connectionString)
        {
            var connection = new SQLiteConnection(connectionString);
            await connection.OpenAsync();

            return new SQLiteFeatureEditSession(fdb, connection, connection.BeginTransaction());
        }

        public string LastErrorMessage
        {
            get => _fdb.LastErrorMessage;
            set => _fdb.LastErrorMessage = value;
        }

        public Task<bool> Insert(IFeatureClass fClass, List<IFeature> features, bool returnIds = false)
            => Run(_fdb.InsertInternal(fClass, features, returnIds, _connection, _transaction));

        public Task<bool> Update(IFeatureClass fClass, List<IFeature> features)
            => Run(_fdb.UpdateInternal(fClass, features, _connection, _transaction));

        public Task<bool> Delete(IFeatureClass fClass, int oid)
            => Run(_fdb.DeleteInternal(fClass, "FDB_OID=" + oid, _connection, _transaction));

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

        public async Task<bool> Commit()
        {
            if (_failed || _committed)
            {
                return false;
            }

            try
            {
                _transaction.Commit();
                _committed = true;

                // classic spatial-index rows are written outside the feature transaction
                // (unchanged behaviour); no-op for native geometry storage.
                await _fdb.AddTreeNodes();
                return true;
            }
            catch (Exception ex)
            {
                _fdb.LastErrorMessage = ex.Message;
                return false;
            }
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                if (!_committed)
                {
                    _fdb.DiscardPendingTreeNodes();
                    try { _transaction.Rollback(); } catch { /* connection may already be gone */ }
                }
            }
            finally
            {
                _transaction.Dispose();
                _connection.Dispose();
            }

            return ValueTask.CompletedTask;
        }
    }
}
