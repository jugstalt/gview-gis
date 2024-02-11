using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Core.Extensions;

static public class ExplorerObjectExtensions
{
    static public IEnumerable<IExplorerObject> GetAncestors(this IExplorerObject exObject, bool includeSelf)
    {
        List<IExplorerObject> ancestors = new List<IExplorerObject>();

        if (includeSelf)
        {
            ancestors.Add(exObject);
        }

        while (exObject.ParentExplorerObject != null)
        {
            ancestors.Insert(0, exObject.ParentExplorerObject);
            exObject = exObject.ParentExplorerObject;
        }

        return ancestors;
    }

    async static public Task<bool> SecureRefreshAsync(this IExplorerObject exObject)
    {
        if (exObject is IExplorerParentObject parentExObject)
        {
            await parentExObject.SecureRefreshAsync();

            return true;
        }

        return false;
    }

    async static public Task SecureRefreshAsync(this IExplorerParentObject parentExObject)
    {
        using (var mutex = await FuzzyMutexAsync.LockAsync(parentExObject.GetHashCode().ToString()))
        {
            if (parentExObject.RequireRefresh() == false)
            {
                return;
            }

            if (mutex.WasBlocked == false)
            {
                try
                {
                    await parentExObject.Refresh();
                }
                catch (Exception ex)
                {
                    if (!parentExObject.HandleRefreshException(ex))
                    {
                        throw;
                    }
                }
            }
        }
    }

    async static public Task<IEnumerable<IExplorerObject>> SecureChildObjectsAsync(this IExplorerParentObject parentExObject)
    {
        await parentExObject.SecureRefreshAsync();
        return await parentExObject.ChildObjects();
    }

    static public IEnumerable<IExplorerObject> GetParents(this IExplorerObject exObject, bool includeSelf)
    {
        List<IExplorerObject> parents = new();

        if (exObject is not null)
        {
            if (includeSelf)
            {
                parents.Add(exObject);
            }

            var parent = exObject.ParentExplorerObject;
            while (parent is not null)
            {
                parents.Add(parent);
                parent = parent.ParentExplorerObject;
            }
        }

        return parents;
    }

    static public IExplorerRootObject? GetRoot(this IExplorerObject exObject)
    {
        while(exObject?.ParentExplorerObject is not null)
        {
            exObject = exObject.ParentExplorerObject;
        }

        return exObject as IExplorerRootObject;
    }
}
