using gView.Framework.Core.Common;
using gView.Framework.Core.IO;

namespace gView.Framework.Core.Carto
{
    public interface IMapServiceProperties : IClone<IMapServiceProperties>, IPersistable
    {
        public int MaxImageWidth { get; }
        public int MaxImageHeight { get; }
        public int MaxRecordCount { get; }
    }
}