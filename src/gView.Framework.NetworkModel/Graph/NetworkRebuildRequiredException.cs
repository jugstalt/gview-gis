namespace gView.Framework.NetworkModel.Graph;

/// <summary>
/// A built network lacks a file that newer versions write (e.g. the spatial index of the nodes, the ends of the edges):
/// it was built by an older version and has to be built again. A <see cref="FileNotFoundException"/>, so callers that
/// catch those keep working.
/// </summary>
public sealed class NetworkRebuildRequiredException(string message) : FileNotFoundException(message)
{
}
