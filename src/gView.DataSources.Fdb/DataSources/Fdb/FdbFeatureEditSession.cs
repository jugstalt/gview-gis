#nullable enable

using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb
{
    /// <summary>
    /// A transactional edit batch over one ADO.NET based FDB (PostgreSQL / SQL Server): every
    /// insert/update/delete runs on the same <see cref="DbConnection"/> and
    /// <see cref="DbTransaction"/>. <see cref="Commit"/> makes them permanent; disposing without
    /// a successful commit rolls everything back.
    /// </summary>
    /// <remarks>
    /// The concrete FDB provides its <c>...Internal</c> methods as delegates so this class stays
    /// provider agnostic.
    /// </remarks>
    public sealed class FdbFeatureEditSession : IFeatureEditSession
    {
        public delegate Task<bool> InsertOp(IFeatureClass fClass, List<IFeature> features, bool returnIds,
                                            DbConnection connection, DbTransaction transaction);
        public delegate Task<bool> UpdateOp(IFeatureClass fClass, List<IFeature> features,
                                            DbConnection connection, DbTransaction transaction);
        public delegate Task<bool> DeleteOp(IFeatureClass fClass, string where,
                                            DbConnection connection, DbTransaction transaction);

        private readonly IErrorMessage _errorSource;
        private readonly Func<int, string> _oidWhere;
        private readonly DbConnection _connection;
        private readonly DbTransaction _transaction;
        private readonly InsertOp _insert;
        private readonly UpdateOp _update;
        private readonly DeleteOp _delete;
        private bool _committed;
        private bool _failed;

        private FdbFeatureEditSession(IErrorMessage errorSource, Func<int, string> oidWhere,
                                     DbConnection connection, DbTransaction transaction,
                                     InsertOp insert, UpdateOp update, DeleteOp delete)
        {
            _errorSource = errorSource;
            _oidWhere = oidWhere;
            _connection = connection;
            _transaction = transaction;
            _insert = insert;
            _update = update;
            _delete = delete;
        }

        public static async Task<FdbFeatureEditSession> CreateAsync(
            IErrorMessage errorSource, DbProviderFactory factory, string connectionString,
            InsertOp insert, UpdateOp update, DeleteOp delete, Func<int, string> oidWhere)
        {
            var connection = factory.CreateConnection()
                ?? throw new Exception("Can't create db connection");
            connection.ConnectionString = connectionString;
            await connection.OpenAsync();

            return new FdbFeatureEditSession(errorSource, oidWhere, connection, connection.BeginTransaction(),
                                             insert, update, delete);
        }

        public string LastErrorMessage
        {
            get => _errorSource.LastErrorMessage;
            set => _errorSource.LastErrorMessage = value;
        }

        public Task<bool> Insert(IFeatureClass fClass, List<IFeature> features, bool returnIds = false)
            => Run(_insert(fClass, features, returnIds, _connection, _transaction));

        public Task<bool> Update(IFeatureClass fClass, List<IFeature> features)
            => Run(_update(fClass, features, _connection, _transaction));

        public Task<bool> Delete(IFeatureClass fClass, int oid)
            => Run(_delete(fClass, _oidWhere(oid), _connection, _transaction));

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
                _errorSource.LastErrorMessage = ex.Message;
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
