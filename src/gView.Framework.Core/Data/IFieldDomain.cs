namespace gView.Framework.Core.Data
{
    //public enum getFeatureQueryType { Geometry, Attributes, All }

    //public enum FieldDomainType { Range, Values, Lookup }
    public interface IFieldDomain : IO.IPersistable, Common.IClone
    {
        string Name { get; }
    }

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
