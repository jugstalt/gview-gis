using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public interface IValuesFieldDomain : IFieldDomain
    {
        Task<object[]> ValuesAsync();
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
