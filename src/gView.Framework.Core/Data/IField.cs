namespace gView.Framework.Core.Data
{
    public interface IField : IO.IMetadata
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
