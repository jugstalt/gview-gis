namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerRootObject : IExplorerObject
{
    string? FileFilter { get; }
}
