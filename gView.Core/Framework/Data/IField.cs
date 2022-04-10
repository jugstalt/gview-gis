namespace gView.Framework.Data
{
    public interface IField : gView.Framework.IO.IMetadata
    {
        string name
        {
            get;
        }

        string aliasname
        {
            get;
        }
        int precision
        {
            get;
        }
        int size
        {
            get;
        }
        FieldType type
        {
            get;
        }

        bool visible { get; set; }
        bool IsRequired { get; }
        bool IsEditable { get; }
        object DefautValue { get; }

        IFieldDomain Domain { get; }
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
