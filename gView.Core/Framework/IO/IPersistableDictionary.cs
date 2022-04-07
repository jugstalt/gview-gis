namespace gView.Framework.IO
{
    public interface IPersistableDictionary : IPersistable
    {
        object this[string key] { get; set; }
    }
}
