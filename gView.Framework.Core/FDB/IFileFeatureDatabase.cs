using gView.Framework.Core.Data;

namespace gView.Framework.Core.FDB
{
    public interface IFileFeatureDatabase : IFeatureDatabase, IFeatureUpdater
    {
        bool Flush(IFeatureClass fc);

        string DatabaseName { get; }
        int MaxFieldNameLength { get; }
    }
}
