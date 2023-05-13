using gView.Blazor.Core;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.SqLite;

[RegisterPlugIn("A4F900EC-C5E4-4518-BAB9-213AF660E8F1")]
internal class SqLiteFdbExplorerObject : ExplorerParentObject, IExplorerFileObject, IExplorerObjectCommandParameters, ISerializableExplorerObject, IExplorerObjectDeletable, IExplorerObjectCreatable
{
    private string _filename = "", _errMsg = "";

    public SqLiteFdbExplorerObject() : base(null, null, 2) { }
    public SqLiteFdbExplorerObject(IExplorerObject? parent, string filename)
        : base(parent, null, 2)
    {
        _filename = filename;
    }

    #region IExplorerObject Members

    public string Filter
    {
        get { return "*.fdb"; }
    }

    public string Name
    {
        get
        {
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(_filename);
                return fi.Name;
            }
            catch { return ""; }
        }
    }

    public string FullName
    {
        get
        {
            return _filename;
        }
    }

    public string Type
    {
        get { return "SQLite Feature Database"; }
    }

    public string Icon => "basic:database";

    public Task<object?> GetInstanceAsync()
    {
        return Task.FromResult<object?>(null);
    }

    async public Task<IExplorerFileObject?> CreateInstance(IExplorerObject? parent, string filename)
    {
        string f = filename.ToLower();
        if (!f.ToLower().EndsWith(".fdb"))
        {
            return null;
        }

        try
        {
            if (!(new FileInfo(f).Exists))
            {
                return null;
            }

            using (SQLiteFDB fdb = new SQLiteFDB())
            {
                if (!await fdb.Open(f) || !await fdb.IsValidAccessFDB())
                {
                    return null;
                }
            }
        }
        catch { return null; }

        return new SqLiteFdbExplorerObject(parent, filename);
    }
    #endregion

    async private Task<string[]> DatasetNames()
    {
        try
        {
            SQLiteFDB fdb = new SQLiteFDB();
            if (!await fdb.Open(_filename))
            {
                _errMsg = fdb.LastErrorMessage;
                return Array.Empty<string>();
            }
            string[] ds = await fdb.DatasetNames();
            string[] dsMod = new string[ds.Length];

            int i = 0;
            foreach (string dsname in ds)
            {
                var isImageDatasetResult = await fdb.IsImageDataset(dsname);
                string imageSpace = isImageDatasetResult.imageSpace;
                if (isImageDatasetResult.isImageDataset)
                {
                    dsMod[i++] = "#" + dsname;
                }
                else
                {
                    dsMod[i++] = dsname;
                }
            }
            if (ds == null)
            {
                _errMsg = fdb.LastErrorMessage;
            }

            fdb.Dispose();

            return dsMod;
        }
        catch
        {
            throw;
        }
    }

    #region IExplorerParentObject Members

    async public override Task<bool> Refresh()
    {
        await base.Refresh();
        string[] ds = await DatasetNames();
        if (ds != null)
        {
            foreach (string dsname in ds)
            {
                if (dsname == "")
                {
                    continue;
                }

                base.AddChildObject(new SqLiteFdbDatasetExplorerObject(this, _filename, dsname));
            }
        }

        return true;
    }

    #endregion

    #region IExplorerObjectCommandParameters Members

    public Dictionary<string, string> Parameters
    {
        get
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("SQLiteFDB", _filename);
            return parameters;
        }
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        IExplorerObject? obj = (cache.Contains(FullName)) ? cache[FullName] : await CreateInstance(null, FullName);

        if (obj != null)
        {
            cache.Append(obj);
        }

        return obj;
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        if (parentExObject == null)
        {
            return false;
        }

        return (PlugInManager.PlugInID(parentExObject) == KnownExplorerObjectIDs.Directory);
    }

    public Task<IExplorerObject?> CreateExplorerObject(IExplorerObject parentExObject)
    {
        if (!CanCreate(parentExObject))
        {
            return Task.FromResult<IExplorerObject?>(null);
        }

        /*
        SaveFileDialog dlg = new SaveFileDialog();
        dlg.Title = "New SQLite Feature Database...";
        dlg.Filter = "SQLite DB(*.fdb)|*.fdb";
        dlg.InitialDirectory = parentExObject.FullName;
        dlg.FileName = "database1.fdb";

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            SQLiteFDB fdb = new SQLiteFDB();
            if (!fdb.Create(dlg.FileName))
            {
                MessageBox.Show(fdb.LastErrorMessage, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Task.FromResult<IExplorerObject>(null);
            }
            _filename = dlg.FileName;
            return Task.FromResult<IExplorerObject>(this);
        }
        */
        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted = null;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        try
        {
            FileInfo fi = new FileInfo(_filename);
            fi.Delete();
            if (ExplorerObjectDeleted != null)
            {
                ExplorerObjectDeleted(this);
            }

            return Task.FromResult(true);
        }
        catch
        {
            throw;
        }
    }

    #endregion
}
