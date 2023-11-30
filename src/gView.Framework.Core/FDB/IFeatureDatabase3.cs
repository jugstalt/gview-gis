using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    public interface IFeatureDatabase3 : IFeatureDatabase2
    {
        Task<int> DatasetID(string dsname);
        Task<int> FeatureClassID(int datasetId, string fcname);
        Task<int> GetFeatureClassID(string fcname);
    }
}
