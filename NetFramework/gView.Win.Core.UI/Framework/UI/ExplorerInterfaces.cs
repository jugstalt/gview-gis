using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.UI
{
    public interface IExplorerIcon
    {
        global::System.Guid GUID { get; }
        global::System.Drawing.Image Image { get; }
    }

    public interface IExplorerObject : IDisposable, ISerializableExplorerObject
    {
        string Name { get; }
        string FullName { get; }
        string Type { get; }

        IExplorerIcon Icon { get; }
        IExplorerObject ParentExplorerObject { get; }

        object Object { get; }
        Type ObjectType { get; }

        int Priority { get; }
    }

    public interface IExporerOjectSchema
    {
        string Schema { get; }
    }

    public interface ISerializableObject
    {
        void DeserializeObject(string config);
        string SerializeObject();
    }

    public interface ISerializableExecute : ISerializableObject
    {
        void Execute(ProgressReporterEvent reporter);
    }

    public interface ISerializableExplorerObject
    {
        IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache);
    }

    public interface ISerializableExplorerObjectCache
    {
        void Append(IExplorerObject exObject);
        bool Contains(string FullName);
        IExplorerObject this[string FullName] { get; }
    }

    public interface IExplorerObjectSerialization
    {
        global::System.Guid Guid { get; }
        string FullName { get; }
        List<global::System.Type> ExplorerObjectTypes { get; }
        List<global::System.Type> ObjectTypes { get; }
    }

    public interface IExplorerFileObject : IExplorerObject
    {
        string Filter { get; }
        IExplorerFileObject CreateInstance(IExplorerObject parent, string filename);
    }

    public interface IExplorerSimpleObject : IExplorerObject
    {

    }

    public interface IExplorerGroupObject : IExplorerObject
    {

    }

    public delegate void RefreshedEventHandler(object sender);
    public interface IRefreshedEventHandler
    {
        event RefreshedEventHandler Refreshed;
    }

    public class KnownExplorerObjectIDs
    {
        private static global::System.Guid _Any = new global::System.Guid("00000000-0000-0000-0000-000000000000");
        private static global::System.Guid _Directory = new global::System.Guid("458E62A0-4A93-45cf-B14D-2F958D67E522");
        private static global::System.Guid _Drive = new global::System.Guid("CB2915F4-DB1A-461a-A14E-73F3A259F0BA");

        public static global::System.Guid Any { get { return _Any; } }
        public static global::System.Guid Directory { get { return _Directory; } }
        public static global::System.Guid Drive { get { return _Drive; } }
    }

    public interface IExplorerParentObject
    {
        List<IExplorerObject> ChildObjects { get; }
        void Refresh();
        void DiposeChildObjects();
    }

    public interface IExplorerObjectTreeNode
    {
        IExplorerObject ExplorerObject
        {
            get;
        }
        IExplorerObjectTreeNode Parent
        {
            get;
        }
        //List<IExplorerObjectTreeNode> Nodes
        //{
        //    get;
        //}
    }

    public class ExplorerObjectEventArgs
    {
        //public IExplorerObjectTreeNode Node;
        public IExplorerObject NewExplorerObject = null;
    }

    public interface IExplorerObjectDoubleClick
    {
        void ExplorerObjectDoubleClick(ExplorerObjectEventArgs e);
    }

    public delegate void ExplorerObjectRenamedEvent(IExplorerObject exObject);
    public interface IExplorerObjectRenamable
    {
        event ExplorerObjectRenamedEvent ExplorerObjectRenamed;
        bool RenameExplorerObject(string newName);
    }

    public delegate void ExplorerObjectDeletedEvent(IExplorerObject exObject);
    public interface IExplorerObjectDeletable
    {
        event ExplorerObjectDeletedEvent ExplorerObjectDeleted;
        bool DeleteExplorerObject(ExplorerObjectEventArgs e);
    }

    public delegate void ExplorerObjectCreatedEvent(IExplorerObject exObject);
    public interface IExplorerObjectCreatable
    {
        //event ExplorerObjectCreatedEvent ExplorerObjectCreated;
        bool CanCreate(IExplorerObject parentExObject);
        IExplorerObject CreateExplorerObject(IExplorerObject parentExObject);
    }



    public interface IExplorerObjectCommandParameters
    {
        Dictionary<string, string> Parameters { get; }
    }
}
