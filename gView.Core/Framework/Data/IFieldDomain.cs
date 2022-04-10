namespace gView.Framework.Data
{
    //public enum getFeatureQueryType { Geometry, Attributes, All }

    //public enum FieldDomainType { Range, Values, Lookup }
    public interface IFieldDomain : IO.IPersistable, gView.Framework.system.IClone
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
