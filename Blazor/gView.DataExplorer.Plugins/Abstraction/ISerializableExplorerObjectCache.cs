namespace gView.DataExplorer.Plugins.Abstraction;

public interface ISerializableExplorerObjectCache
{
    void Append(IExplorerObject exObject);
    bool Contains(string FullName);
    IExplorerObject this[string FullName] { get; }
}
