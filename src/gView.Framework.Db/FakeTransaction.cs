using System.Data;
using System.Data.Common;

namespace gView.Framework.Db
{
    public class FakeTransaction : DbTransaction
    {
        public FakeTransaction(DbConnection connection)
        {
            _connection = connection;
        }

        public override void Commit()
        {
            // Do Nothing
        }

        public override void Rollback()
        {
            // Do Nothing
        }

        public override IsolationLevel IsolationLevel => IsolationLevel.Chaos;

        private DbConnection _connection;
        protected override DbConnection DbConnection => _connection;

        protected override void Dispose(bool disposing)
        {
            // Do Nothing
        }
    }
}
