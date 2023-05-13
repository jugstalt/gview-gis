using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Base;

public class ExplorerParentObject : ExplorerObjectCls, IExplorerParentObject
{
    private List<IExplorerObject>? _childObjects = null;

    public ExplorerParentObject(IExplorerObject? parent, Type? type, int priority)
        : base(parent, type, priority)
    {
    }

    #region IExplorerParentObject Member

    async virtual public Task<List<IExplorerObject>> ChildObjects()
    {
        if (_childObjects == null)
        {
            await Refresh();
        }

        return _childObjects ?? new List<IExplorerObject>();
    }

    async virtual public Task<bool> Refresh()
    {
        await this.DiposeChildObjects();
        _childObjects = new List<IExplorerObject>();

        return true;
    }

    public Task<bool> DiposeChildObjects()
    {
        if (_childObjects == null)
        {
            return Task.FromResult(false);
        }

        foreach (IExplorerObject exObject in _childObjects)
        {
            if (exObject == null)
            {
                continue;
            }

            exObject.Dispose();
        }
        _childObjects = null;

        return Task.FromResult(true);
    }

    #endregion

    protected void RemoveAllChildObjects()
    {
        if (_childObjects != null)
        {
            _childObjects.Clear();
        }
    }

    protected void AddChildObject(IExplorerObject child)
    {
        if (child == null)
        {
            return;
        }

        if (_childObjects == null)
        {
            _childObjects = new List<IExplorerObject>();
        }

        if (child is IExplorerObjectDeletable)
        {
            ((IExplorerObjectDeletable)child).ExplorerObjectDeleted += new ExplorerObjectDeletedEvent(Child_ExplorerObjectDeleted);
        }

        _childObjects.Add(child);
    }

    protected void SortChildObjects(IComparer<IExplorerObject> comparer)
    {
        if (_childObjects == null || comparer == null)
        {
            return;
        }

        _childObjects.Sort(comparer);
    }
    void Child_ExplorerObjectDeleted(IExplorerObject exObject)
    {
        IExplorerObject? delExObject = null;

        if (_childObjects != null)
        {
            foreach (IExplorerObject child in _childObjects)
            {
                if (ExObjectComparer.Equals(child, exObject))
                {
                    delExObject = child;
                    break;
                }
            }
        }

        if (delExObject != null && _childObjects != null)
        {
            _childObjects.Remove(delExObject);
        }
    }

    virtual public void Dispose()
    {
        DiposeChildObjects();
    }
}
