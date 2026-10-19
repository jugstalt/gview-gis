using gView.Framework.Core.Common;
using gView.Framework.Core.IO;

namespace gView.Framework.Core.Carto
{
    public interface IMapServiceProperties : IClone<IMapServiceProperties>, IPersistable
    {
        public int MaxImageWidth { get; }
        public int MaxImageHeight { get; }
        public int MaxRecordCount { get; }
        /// <summary>Max. number of source records scanned for in-memory statistics queries (0 = default 1,000,000).</summary>
        public int MaxStatisticsRecordScan { get; }
    }
}