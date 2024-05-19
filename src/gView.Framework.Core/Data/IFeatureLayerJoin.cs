using gView.Framework.Core.Carto;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IFeatureLayerJoin : IPersistable, IDisposable, IClone
    {
        string Name { get; }

        string JoinName { get; set; }

        string Field { get; set; }

        IFieldCollection JoinFields { get; set; }
        Task<IRow> GetJoinedRow(string val);
        Task PerformCacheQuery(string[] vals);
        Task<ICursor> PerformQuery(IQueryFilter filter);

        void Init(string selectFieldNames);

        joinType JoinType { get; set; }

        void OnCreate(IMap map);
    }
}