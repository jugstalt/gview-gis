﻿using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.Network;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.SqLite;

[RegisterPlugIn("EEDCCBB2-588E-418A-B048-4B6C210A25AE")]
public class SqLiteFdbLinkedFeatureclassExplorerObject : IExplorerSimpleObject, 
                                                         IExplorerObjectCreatable
{
    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        return parentExObject is SqLiteFdbDatasetExplorerObject;
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IApplicationScope scope, IExplorerObject? parentExObject)
    {
        SqLiteFdbDatasetExplorerObject? parent = (SqLiteFdbDatasetExplorerObject?)parentExObject;
        if (parent == null)
        {
            return null;
        }

        IFeatureDataset? dataset = await parent.GetInstanceAsync() as IFeatureDataset;
        if (dataset == null)
        {
            return null;
        }

        AccessFDB? fdb = dataset.Database as AccessFDB;
        if (fdb == null)
        {
            return null;
        }

        throw new KeyNotFoundException();

        //List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
        //filters.Add(new OpenFeatureclassFilter());
        //ExplorerDialog dlg = new ExplorerDialog("Select Featureclass", filters, true);

        //IExplorerObject ret = null;

        //if (dlg.ShowDialog() == DialogResult.OK &&
        //    dlg.ExplorerObjects != null)
        //{
        //    foreach (IExplorerObject exObj in dlg.ExplorerObjects)
        //    {
        //        var exObjectInstance = await exObj?.GetInstanceAsync();
        //        if (exObjectInstance is IFeatureClass)
        //        {
        //            int fcid = await fdb.CreateLinkedFeatureClass(dataset.DatasetName, (IFeatureClass)exObjectInstance);
        //            if (ret == null)
        //            {
        //                IDatasetElement element = await dataset.Element(((IFeatureClass)exObjectInstance).Name);
        //                if (element != null)
        //                {
        //                    ret = new SqLiteFdbFeatureClassExplorerObject(
        //                        parent,
        //                        parent.FileName,
        //                        parent.Name,
        //                        element);
        //                }

        //            }
        //        }
        //    }
        //}
        //return ret;
    }

    #endregion

    #region IExplorerObject Member

    public string Name
    {
        get { return "Linked Featureclass"; }
    }

    public string FullName
    {
        get { return "Linked Featureclass"; }
    }

    public string Type
    {
        get { return String.Empty; }
    }

    public string Icon => "basic:open-in-window";

    public IExplorerObject? ParentExplorerObject
    {
        get { return null; }
    }

    public Task<object?> GetInstanceAsync()
    {
        return Task.FromResult<object?>(null);
    }

    public Type? ObjectType
    {
        get { return null; }
    }

    public int Priority { get { return 1; } }

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
}