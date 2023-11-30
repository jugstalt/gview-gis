namespace gView.Framework.Core.Data
{
    public interface IFieldValue
    {
        string Name { get; }
        object Value { get; set; }
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
