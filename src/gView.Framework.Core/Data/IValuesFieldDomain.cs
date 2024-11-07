using System.Threading.Tasks;

namespace gView.Framework.Core.Data
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
