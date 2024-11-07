using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IDBOperations
    {
        Task<bool> BeforeInsert(ITableClass tClass);
        Task<bool> BeforeUpdate(ITableClass tClass);
        Task<bool> BeforeDelete(ITableClass tClass);
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
