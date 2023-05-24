using gView.Framework.DataExplorer.Abstraction;
using System;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Base;

public class ExplorerObjectCls
{
    protected IExplorerObject? _parent;
    private Type? _type = typeof(object);

    public ExplorerObjectCls(IExplorerObject? parent, Type? type, int priority)
    {
        _parent = parent;
        _type = type;
        this.Priority = priority;
    }

    public IExplorerObject? ParentExplorerObject
    {
        get { return _parent; }
    }

    public Type? ObjectType
    {
        get { return _type; }
    }

    public int Priority { get; }
}
