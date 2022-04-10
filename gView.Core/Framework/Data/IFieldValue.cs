namespace gView.Framework.Data
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
