using gView.Framework.DataExplorer.Abstraction;
using MudBlazor;

namespace gView.DataExplorer.Razor.Components.Trees;

internal class ExplorerTreeNode : TreeItemData<IExplorerObject>
{
    public ExplorerTreeNode(IExplorerObject exObject)
    {
        this.Value = exObject;

        base.Text = exObject.Name;
        base.Icon = exObject.Icon;
        base.Expanded = false;

        if (exObject is IExplorerParentObject)
        {
            base.Children = new()
            {
                new ExplorerTreeNode(DummyObjectInstance)
            };
        }
    }

    public bool IsServerLoaded
        => (this.Children is null
            || this.Children.FirstOrDefault()?.Value is DummyExplorerObject) == false;

    public override bool Equals(TreeItemData<IExplorerObject>? other)
    {
        return this.Value == other?.Value;
    }

    static private DummyExplorerObject DummyObjectInstance { get; } = new DummyExplorerObject();

    #region Classes

    private class DummyExplorerObject : IExplorerObject
    {
        public string Name => string.Empty;

        public string FullName => string.Empty;

        public string? Type => string.Empty;

        public string Icon => string.Empty;

        public IExplorerObject? ParentExplorerObject => null;

        public Type? ObjectType => null;

        public int Priority => 0;

        public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
        {
            return Task.FromResult<IExplorerObject?>(null);
        }

        public void Dispose()
        {

        }

        public Task<object?> GetInstanceAsync()
        {
            return Task.FromResult<object?>(null);
        }

        public override string ToString() => "...";
    }


    #endregion
}
