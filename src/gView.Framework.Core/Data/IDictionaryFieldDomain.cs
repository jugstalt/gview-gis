using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface IDictionaryFieldDomain : IFieldDomain
    {
        Dictionary<string /* tag */, object /* dbValue */> Dictionary { get; }
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
