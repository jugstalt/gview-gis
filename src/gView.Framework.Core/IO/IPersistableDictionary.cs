namespace gView.Framework.Core.IO
{
    public interface IPersistableDictionary : IPersistable
    {
        object this[string key] { get; set; }
    }
}
