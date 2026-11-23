using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Credentials;
using gView.Pipe.Abstraction.Nodes;
using System.ComponentModel.DataAnnotations;

namespace gView.Pipe.Plugins.Gis;

/// <summary>
/// A network built with gView (Carto, server administration or gView.Cmd): the folder with network.json, shards and indexes.
/// Network Trace searches it; Pipe never builds it.
/// </summary>
[PipeCredentialType(TypeId, "gView Network",
    Description = "A network built with gView GIS (Carto, server or gView.Cmd) for Network Trace")]
public sealed class GisNetworkCredential
{
    public const string TypeId = "3ba13171-ccf1-488f-89e0-ca87df92f6d1";

    [Required, PipeParameter(Label = "Network directory", Order = 1,
        Description = "The folder of the built network on the Pipe machine, the one holding network.json, e.g. D:\\gview\\maps\\grid.network")]
    public string Directory { get; set; } = "";

    /// <summary>The full path of the built network; throws when it isn't there or not built.</summary>
    public string BuiltDirectory()
    {
        if (string.IsNullOrWhiteSpace(Directory))
        {
            throw new PipeException("The gView Network credential has no network directory");
        }
        var directory = Path.GetFullPath(Directory.Trim());
        if (!File.Exists(Path.Combine(directory, "network.json")))
        {
            throw new PipeException($"No built network in {directory} (network.json is missing); build it in Carto, the server administration or with gView.Cmd");
        }
        return directory;
    }
}
