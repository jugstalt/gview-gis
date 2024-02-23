using gView.Framework.DataExplorer.Abstraction;

namespace gView.DataExplorer.Plugins.ExplorerObjects.FileSystem;

internal class EnvironmentVariableDrive : DriveObject
{
    public EnvironmentVariableDrive(IExplorerObject parent, string name, string path)
        : base(parent, name, path, 998)
    {
    }
}
