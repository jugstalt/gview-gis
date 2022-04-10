using System.Threading.Tasks;

namespace gView.Framework.Data
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
