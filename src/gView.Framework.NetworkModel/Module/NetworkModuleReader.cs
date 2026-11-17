using gView.Framework.Core.Carto;
using gView.Framework.Core.IO;

namespace gView.Framework.NetworkModel.Module;

/// <summary>
/// Reads the <see cref="NetworkModule"/> of a map document without the application that wrote it:
/// MapDocument / Moduls / Module (GUID = <see cref="NetworkModule.PluginGuid"/>).
/// </summary>
public sealed class NetworkModuleReader : IPersistable
{
    private readonly NetworkModule _module = new();

    private NetworkModuleReader(IMap? map) { if (map is not null) _module.OnCreate(map); }

    /// <summary>The network module stored in the document, or null when the document has none (or it is empty).</summary>
    /// <param name="loadMapDocument">Loads the element "MapDocument" with the given reader, e.g. from a fresh XmlStream.</param>
    public static NetworkModule? Read(IMap? map, Action<IPersistable> loadMapDocument)
    {
        var reader = new NetworkModuleReader(map);
        loadMapDocument(reader);
        return reader._found && reader._module.IsConfigured ? reader._module : null;
    }

    private bool _found;

    public void Load(IPersistStream stream) => stream.Load("Moduls", null, new ModuleList(this));

    public void Save(IPersistStream stream) => throw new NotSupportedException();

    private sealed class ModuleList(NetworkModuleReader reader) : IPersistable
    {
        public void Load(IPersistStream stream)
        {
            while (stream.Load("Module", null, new ModuleEntry(reader)) is not null)
            {
            }
        }

        public void Save(IPersistStream stream) => throw new NotSupportedException();
    }

    private sealed class ModuleEntry(NetworkModuleReader reader) : IPersistable
    {
        public void Load(IPersistStream stream)
        {
            if (stream.Load("GUID", null) is string guid
                && Guid.TryParse(guid, out var id)
                && id == Guid.Parse(NetworkModule.PluginGuid))
            {
                reader._module.Load(stream);
                reader._found = true;
            }
        }

        public void Save(IPersistStream stream) => throw new NotSupportedException();
    }
}
