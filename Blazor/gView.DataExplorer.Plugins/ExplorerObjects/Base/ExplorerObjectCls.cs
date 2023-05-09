using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Text;

namespace gView.DataExplorer.Plugins.ExplorerObject.Base;

public class ExplorerObjectCls
{
    private IExplorerObject _parent;
    private Type? _type = null;

    public ExplorerObjectCls(IExplorerObject parent, Type type, int priority)
    {
        _parent = parent;
        _type = type;
        this.Priority = priority;
    }

    public IExplorerObject ParentExplorerObject
    {
        get { return _parent; }
    }

    public Type? ObjectType
    {
        get { return _type; }
    }

    public int Priority { get; }
}
