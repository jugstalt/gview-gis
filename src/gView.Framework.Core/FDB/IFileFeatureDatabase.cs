using gView.Framework.Core.Data;

namespace gView.Framework.Core.FDB
{
    public interface IFileFeatureDatabase : IFeatureDatabase, IFeatureUpdater
    {
        bool Flush(IFeatureClass fc);

        string DatabaseName { get; }
        int MaxFieldNameLength { get; }

        /// <summary>
        /// <c>true</c> when the database <i>is</i> a directory (one file per feature class,
        /// e.g. Shapefile, GML). <c>false</c> for single-file databases (SpatiaLite,
        /// GeoPackage) - callers that create one in a folder then have to ask for a file name.
        /// </summary>
        bool IsFolderBased => true;
    }
}
