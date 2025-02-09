﻿using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Fdb.Extensions;
using gView.DataExplorer.Plugins.Extensions;
using gView.DataSources.Fdb.PostgreSql;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.Common;
using System;
using System.Threading.Tasks;
using gView.Framework.DataExplorer.Services.Abstraction;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.PostgreSql;

[RegisterPlugIn("18830A3E-FFEA-477c-B3E1-E4624F828034")]
public class PostgreSqlNetworkClassExplorerObject : ExplorerObjectCls<IExplorerObject, pgNetworkFeatureClass>,
                                                    IExplorerSimpleObject,
                                                    IExplorerObjectCreatable
{
    public PostgreSqlNetworkClassExplorerObject() : base(1) { }

    #region IExplorerObject Member

    public string Name
    {
        get { return String.Empty; }
    }

    public string FullName
    {
        get { return String.Empty; }
    }

    public string Type
    {
        get { return "Network Class"; }
    }

    public string Icon => "webgis:construct-edge-intersect";

    public Task<object?> GetInstanceAsync()
    {
        return Task.FromResult<object?>(null);
    }

    #endregion

    #region IDisposable Member

    public void Dispose()
    {

    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        if (parentExObject is PostgreSqlDatasetExplorerObject)
        {
            return true;
        }

        return false;
    }

    public async Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScopeService scope, IExplorerObject? parentExObject)
    {
        if (parentExObject == null)
        {
            throw new ArgumentNullException(nameof(parentExObject));
        }

        if (await scope.CreateNetworkClass(parentExObject))
        {
            await scope.ForceContentRefresh();
        }

        return null;
    }

    #endregion
}


