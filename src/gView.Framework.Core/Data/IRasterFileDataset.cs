using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Data
{
    public interface IRasterFileDataset : IRasterDataset
    {
        IRasterLayer AddRasterFile(string filename);
        IRasterLayer AddRasterFile(string filename, IPolygon polygon);

        string SupportedFileFilter { get; }
        int SupportsFormat(string extension);
    }
}