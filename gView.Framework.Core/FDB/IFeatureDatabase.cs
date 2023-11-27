using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFeatureDatabase : IDatabase, IFeatureUpdater
    {
        //int OpenDataset(string name);
        //int OpenFeatureClass(int DatasetID,string name);

        Task<int> CreateDataset(string name, ISpatialReference sRef);
        Task<int> CreateFeatureClass(
            string dsname,
            string fcname,
            IGeometryDef geomDef,
            IFieldCollection Fields);

        Task<IFeatureDataset> GetDataset(string name);

        Task<bool> DeleteDataset(string dsName);
        Task<bool> DeleteFeatureClass(string fcName);

        Task<bool> RenameDataset(string name, string newName);
        Task<bool> RenameFeatureClass(string name, string newName);

        Task<IFeatureCursor> Query(IFeatureClass fc, IQueryFilter filter);

        Task<string[]> DatasetNames();
    }
}
