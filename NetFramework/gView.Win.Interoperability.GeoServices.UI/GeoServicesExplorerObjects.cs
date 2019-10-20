using gView.Framework.Globalisation;
using gView.Framework.IO;
using gView.Framework.system.UI;
using gView.Framework.UI;
using System;
using System.Threading.Tasks;
using gView.Framework.system;
using System.Xml;
using gView.Framework.Web;
using gView.Interoperability.GeoServices.Rest.Json;
using System.Linq;

namespace gView.Interoperability.GeoServices.Dataset
{
    [gView.Framework.system.RegisterPlugIn("517DEC80-F6A5-44BC-95EF-9A56543C373B")]
    public class GeoServicesExplorerObjects : ExplorerParentObject, IExplorerGroupObject
    {
        IExplorerIcon _icon = new GeoServicesIcon();

        public GeoServicesExplorerObjects() : base(null, null, 0) { }

        #region IExplorerObject Member

        public string Name
        {
            get { return "ESRI GeoServices"; }
        }

        public string FullName
        {
            get { return "ESRI.GeoServices"; }
        }

        public string Type
        {
            get { return "ESRI.GeoServices Connections"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerParentObject Member

        async public override Task<bool> Refresh()
        {
            await base.Refresh();
            base.AddChildObject(new GeoServicesNewConnectionExplorerObject(this));

            ConfigTextStream stream = new ConfigTextStream("GeoServices_connections");
            string connStr, id;
            while ((connStr = stream.Read(out id)) != null)
            {
                base.AddChildObject(new GeoServicesConnectionExplorerObject(this, id, connStr));
            }
            stream.Close();

            return true;
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return Task.FromResult(cache[FullName]);
            }

            if (FullName == this.FullName)
            {
                GeoServicesExplorerObjects exObject = new GeoServicesExplorerObjects();
                cache.Append(exObject);
                return Task.FromResult<IExplorerObject>(exObject);
            }
            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("573EA02F-778C-4F9A-9DA6-DF9DBE3978D6")]
    public class GeoServicesNewConnectionExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectDoubleClick, IExplorerObjectCreatable
    {
        IExplorerIcon _icon = new GeoServicesNewConnectionIcon();

        public GeoServicesNewConnectionExplorerObject()
            : base(null, null, 0)
        {
        }

        public GeoServicesNewConnectionExplorerObject(IExplorerObject parent)
            : base(parent, null, 0)
        {
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return LocalizedResources.GetResString("String.NewConnection", "New GeoServices Connection..."); }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return ""; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {

        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerObjectDoubleClick Member

        public void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e)
        {
            FormNewConnection dlg = new FormNewConnection();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string connStr = dlg.ConnectionString;
                ConfigTextStream stream = new ConfigTextStream("GeoServices_connections", true, true);
                string id = ConfigTextStream.ExtractValue(connStr, "server").UrlToConfigId();
                stream.Write(connStr, ref id);
                stream.Close();

                e.NewExplorerObject = new GeoServicesConnectionExplorerObject(this.ParentExplorerObject, id, dlg.ConnectionString);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return  Task.FromResult(cache[FullName]);
            }

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is GeoServicesExplorerObjects);
        }

        public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            ExplorerObjectDoubleClick(e);
            return Task.FromResult(e.NewExplorerObject);
        }

        #endregion
    }

    internal class GeoServicesConnectionExplorerObject : ExplorerParentObject, IExplorerSimpleObject, IExplorerObjectDeletable, IExplorerObjectRenamable
    {
        private string _name = "", _connectionString = "";
        private IExplorerIcon _icon = new GeoServicesConnectionIcon();

        public GeoServicesConnectionExplorerObject() : base(null, null, 0) { }
        internal GeoServicesConnectionExplorerObject(IExplorerObject parent, string name, string connectionString)
            : base(parent, null, 0)
        {
            _name = name;
            _connectionString = connectionString;
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return _name; }
        }

        public string FullName
        {
            get { return @"gView.GeoServices\" + _name; }
        }

        public string Type
        {
            get { return "gView.GeoServices Connection"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }
        #endregion

        #region IExplorerParentObject Member

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            try
            {
                string server = ConfigTextStream.ExtractValue(_connectionString, "server");
                string usr = ConfigTextStream.ExtractValue(_connectionString, "user");
                string pwd = ConfigTextStream.ExtractValue(_connectionString, "pwd");

                // ToDo: GetToken if usr, pwd

                var jsonServices = await WebFunctions.DownloadObjectAsync<JsonServices>(server.UrlAppendParameters("f=json"));

                if (jsonServices != null)
                {
                    if (jsonServices.Folders != null)
                    {
                        foreach (var folder in jsonServices.Folders)
                        {
                            base.AddChildObject(
                                new GeoServicesFolderExplorerObject(
                                    this,
                                    folder,
                                    _connectionString)
                                );
                        }
                    }
                    if (jsonServices.Services != null)
                    {
                        foreach (var service in jsonServices.Services.Where(s => s.Type.ToLower() == "mapserver"))
                        {
                            base.AddChildObject(
                                new GeoServicesServiceExplorerObject(
                                    this,
                                    service.ServiceName,
                                    String.Empty,
                                    _connectionString));
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);

                return false;
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return cache[FullName];
            }

            GeoServicesExplorerObjects group = new GeoServicesExplorerObjects();
            if (FullName.IndexOf(group.FullName) != 0 || FullName.Length < group.FullName.Length + 2)
            {
                return null;
            }

            group = (GeoServicesExplorerObjects)((cache.Contains(group.FullName)) ? cache[group.FullName] : group);

            foreach (IExplorerObject exObject in await group.ChildObjects())
            {
                if (exObject.FullName == FullName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            ConfigTextStream stream = new ConfigTextStream("GeoServices_connections", true, true);
            stream.Remove(this.Name, _connectionString);
            stream.Close();
            if (ExplorerObjectDeleted != null)
            {
                ExplorerObjectDeleted(this);
            }

            return Task.FromResult(true);
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed = null;

        public Task<bool> RenameExplorerObject(string newName)
        {
            bool ret = false;
            ConfigTextStream stream = new ConfigTextStream("GeoServices_connections", true, true);
            ret = stream.ReplaceHoleLine(ConfigTextStream.BuildLine(_name, _connectionString), ConfigTextStream.BuildLine(newName, _connectionString));
            stream.Close();

            if (ret == true)
            {
                _name = newName;
                if (ExplorerObjectRenamed != null)
                {
                    ExplorerObjectRenamed(this);
                }
            }
            return Task.FromResult(ret);
        }

        #endregion
    }

    public class GeoServicesFolderExplorerObject : ExplorerParentObject, IExplorerSimpleObject
    {
        private IExplorerIcon _icon = new GeoServicesServiceIcon();
        private string _name = "", _connectionString = "";
        private GeoServicesClass _class = null;
        private GeoServicesConnectionExplorerObject _parent = null;

        internal GeoServicesFolderExplorerObject(GeoServicesConnectionExplorerObject parent, string name, string connectionString)
            : base(parent, null, 1)
        {
            _name = name;
            _connectionString = connectionString;
            _parent = parent;
        }

        #region IExplorerParentObject Member

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            try
            {
                string server = ConfigTextStream.ExtractValue(_connectionString, "server");
                string usr = ConfigTextStream.ExtractValue(_connectionString, "user");
                string pwd = ConfigTextStream.ExtractValue(_connectionString, "pwd");

                // ToDo: GetToken if usr, pwd

                var jsonServices = await WebFunctions.DownloadObjectAsync<JsonServices>(
                    server
                        .UrlAppendPath(this._name)
                        .UrlAppendParameters("f=json"));

                if (jsonServices != null)
                {
                    if (jsonServices.Folders != null)
                    {
                        foreach (var folder in jsonServices.Folders)
                        {
                            base.AddChildObject(
                                new GeoServicesFolderExplorerObject(
                                    this._parent,
                                    this._name.UrlAppendPath(folder),
                                    _connectionString)
                                );
                        }
                    }
                    if (jsonServices.Services != null)
                    {
                        foreach (var service in jsonServices.Services.Where(s => s.Type.ToLower() == "mapserver"))
                        {
                            base.AddChildObject(
                                new GeoServicesServiceExplorerObject(
                                    this,
                                    service.ServiceName,
                                    this._name,
                                    _connectionString));
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);

                return false;
            }
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get { return _name; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null)
                {
                    return "";
                }

                return _parent.FullName + @"\" + _name;
            }
        }

        public string Type
        {
            get { return "gView.GeoServices Folder"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {

        }

        async public Task<object> GetInstanceAsync()
        {

            return Task.FromResult<object>(null);
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return cache[FullName];
            }

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1)
            {
                return null;
            }

            string cnName = FullName.Substring(0, lastIndex);
            string svName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            GeoServicesConnectionExplorerObject cnObject = new GeoServicesConnectionExplorerObject();
            cnObject = await cnObject.CreateInstanceByFullName(cnName, cache) as GeoServicesConnectionExplorerObject;

            var childObjects = await cnObject?.ChildObjects();
            if (cnObject == null || childObjects == null)
            {
                return null;
            }

            foreach (IExplorerObject exObject in childObjects)
            {
                if (exObject.Name == svName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion
    }

    public class GeoServicesServiceExplorerObject : ExplorerObjectCls, IExplorerSimpleObject
    {
        private IExplorerIcon _icon = new GeoServicesServiceIcon();
        private string _name = "", _connectionString = "", _folder = "";
        private GeoServicesClass _class = null;
        private IExplorerObject _parent = null;

        internal GeoServicesServiceExplorerObject(IExplorerObject parent, string name, string folder, string connectionString)
            : base(parent, typeof(GeoServicesClass), 1)
        {
            _name = name;
            _folder = folder;
            _connectionString = connectionString;
            _parent = parent;
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return _name; }
        }

        public string FullName
        {
            get
            {
                if (_parent == null)
                {
                    return "";
                }

                return _parent.FullName +
                    $@"\{_name}";
            }
        }

        public string Type
        {
            get { return "gView.GeoServices Service"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public void Dispose()
        {

        }

        async public Task<object> GetInstanceAsync()
        {

            if (_class == null)
            {
                GeoServicesDataset dataset = new GeoServicesDataset(
                    _connectionString,
                    (String.IsNullOrWhiteSpace(_folder) ? "" : $"{_folder}/") + _name);
                await dataset.Open(); 

                var elements = await dataset.Elements();
                if (elements.Count == 0)
                {
                    dataset.Dispose();
                    return null;
                }

                _class = elements[0].Class as GeoServicesClass;
            }

            return _class;
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region ISerializableExplorerObject Member

        async public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
            {
                return cache[FullName];
            }

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1)
            {
                return null;
            }

            string cnName = FullName.Substring(0, lastIndex);
            string svName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            GeoServicesConnectionExplorerObject cnObject = new GeoServicesConnectionExplorerObject();
            cnObject = await cnObject.CreateInstanceByFullName(cnName, cache) as GeoServicesConnectionExplorerObject;

            var childObjects = await cnObject?.ChildObjects();
            if (cnObject == null || childObjects == null)
            {
                return null;
            }

            foreach (IExplorerObject exObject in childObjects)
            {
                if (exObject.Name == svName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion
    }

    internal class GeoServicesIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("573EA02F-778C-4F9A-9DA6-DF9DBE3978D6"); }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return gView.Win.Interoperability.GeoServices.UI.Properties.Resources.computer;
            }
        }

        #endregion
    }
    internal class GeoServicesNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("71C61FF9-6D5A-42D8-B069-A4A066BF385E"); }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return gView.Win.Interoperability.GeoServices.UI.Properties.Resources.add_16;
            }
        }

        #endregion
    }
    internal class GeoServicesConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("573EA02F-778C-4F9A-9DA6-DF9DBE3978D6"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Win.Interoperability.GeoServices.UI.Properties.Resources.computer_go; }
        }

        #endregion
    }
    internal class GeoServicesServiceIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("C7406FB4-4F52-4F16-95BE-04EA234FA33A"); }
        }

        public System.Drawing.Image Image
        {
            get { return gView.Win.Interoperability.GeoServices.UI.Properties.Resources.i_connection; }
        }

        #endregion
    }
}
