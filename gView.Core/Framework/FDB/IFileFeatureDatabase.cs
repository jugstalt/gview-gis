using gView.Framework.Data;

namespace gView.Framework.FDB
{
    public interface IFileFeatureDatabase : IFeatureDatabase, IFeatureUpdater
    {
        bool Flush(IFeatureClass fc);

        string DatabaseName { get; }
        int MaxFieldNameLength { get; }
    }
}
