namespace gView.Framework.Data
{
    public interface IRangeFieldDomain : IFieldDomain
    {
        double MinValue { get; }
        double MaxValue { get; }
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
