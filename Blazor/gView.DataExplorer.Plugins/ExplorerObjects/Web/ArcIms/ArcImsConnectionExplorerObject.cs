using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Web.ArcIms.ContextTools;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.IO;
using gView.Interoperability.ArcXML.Dataset;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Web.ArcIms;

internal class ArcImsConnectionExplorerObject : ExplorerParentObject,
                                                IExplorerSimpleObject,
                                                IExplorerObjectDeletable,
                                                IExplorerObjectRenamable,
                                                IExplorerObjectContextTools
{
    private string _name = "", _connectionString = "";
    private IEnumerable<IExplorerObjectContextTool>? _contextTools = null;

    public ArcImsConnectionExplorerObject() : base(null, null, 0) { }
    internal ArcImsConnectionExplorerObject(IExplorerObject parent, string name, string connectionString)
        : base(parent, null, 0)
    {
        _name = name;
        _connectionString = connectionString;

        _contextTools = new IExplorerObjectContextTool[]
        {
            new UpdateConnectionString()
        };
    }

    internal string GetConnectionString() => _connectionString;

    internal Task<bool> UpdateConnectionString(string connectionString)
    {
        ConfigConnections configConnections = new ConfigConnections("arcims_connections", "546B0513-D71D-4490-9E27-94CD5D72C64A");
        configConnections.Add(_name, connectionString);

        _connectionString = connectionString;

        return Task.FromResult(true);
    }

    #region IExplorerObject Member

    public string Name => _name;

    public string FullName => @"ESRI\ArcIMS\" + _name;

    public string Type
    {
        get { return "ESRI ArcIMS Connection"; }
    }

    public string Icon => "basic:globe";

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region IExplorerObjectContextTools

    public IEnumerable<IExplorerObjectContextTool> ContextTools => _contextTools ?? Array.Empty<IExplorerObjectContextTool>();

    #endregion

    #region IExplor#region IExplorerParentObject Member

    async public override Task<bool> Refresh()
    {
        await base.Refresh();

        try
        {
            dotNETConnector connector = new dotNETConnector();

            string server = ConfigTextStream.ExtractValue(_connectionString, "server");
            string usr = ConfigTextStream.ExtractValue(_connectionString, "user");
            string pwd = ConfigTextStream.ExtractValue(_connectionString, "pwd");

            if (usr != "" || pwd != "")
            {
                connector.setAuthentification(usr, pwd);
            }

            string axl = connector.SendRequest("<?xml version=\"1.0\" encoding=\"UTF-8\"?><GETCLIENTSERVICES/>", server, "catalog");
            if (axl == "")
            {
                return false;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(axl);

            foreach (XmlNode mapService in doc.SelectNodes("//SERVICE[@name]"))
            {
                base.AddChildObject(new ArcImsServiceExplorerObject(this, mapService.Attributes["name"].Value, _connectionString));
            }
            if (doc.SelectNodes("//SERVICE[@name]").Count == 0)
            {
                foreach (XmlNode mapService in doc.SelectNodes("//SERVICE[@NAME]"))
                {
                    base.AddChildObject(new ArcImsServiceExplorerObject(this, mapService.Attributes["NAME"].Value, _connectionString));
                }
            }

            return true;
        }
        catch //(Exception ex)
        {
            throw;
        }
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache != null && cache.Contains(FullName))
        {
            return cache[FullName];
        }

        ArcImsExplorerObjects? group = new ArcImsExplorerObjects();
        if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2)
        {
            return null;
        }

        group = (ArcImsExplorerObjects?)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

        if (group != null)
        {
            foreach (IExplorerObject exObject in await group.ChildObjects())
            {
                if (exObject.FullName == FullName)
                {
                    cache?.Append(exObject);
                    return exObject;
                }
            }
        }

        return null;
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        ConfigConnections configConnections = new ConfigConnections("arcims_connections", "546B0513-D71D-4490-9E27-94CD5D72C64A");
        configConnections.Remove(this.Name);

        if (ExplorerObjectDeleted != null)
        {
            ExplorerObjectDeleted(this);
        }

        return Task.FromResult(true);
    }

    #endregion

    #region IExplorerObjectRenamable Member

    public event ExplorerObjectRenamedEvent? ExplorerObjectRenamed;

    public Task<bool> RenameExplorerObject(string newName)
    {
        ConfigConnections configConnections = new ConfigConnections("arcims_connections", "546B0513-D71D-4490-9E27-94CD5D72C64A");
        bool result = configConnections.Rename(this.Name, newName);

        if (result == true)
        {
            _name = newName;
            if (ExplorerObjectRenamed != null)
            {
                ExplorerObjectRenamed(this);
            }
        }
        return Task.FromResult(result);
    }

    #endregion
}
