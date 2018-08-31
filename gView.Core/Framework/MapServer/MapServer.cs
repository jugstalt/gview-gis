using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Carto;
using gView.Framework.system;
using gView.Framework.IO;

namespace gView.MapServer
{
    public interface IMapServer
    {
        List<IMapService> Maps { get; }
        IServiceMap this[string name] { get; }
        IServiceMap this[IMapService service] { get; }
        IServiceMap this[IServiceRequestContext context] { get; }

        bool LoggingEnabled(loggingMethod methode);
        void Log(string header, loggingMethod methode, string msg);

        string OutputUrl { get; }
        string OutputPath { get; }

        string TileCachePath { get; }

        bool CheckAccess(IIdentity identity, string service);
    }

    public enum MapServiceType { MXL, SVC, GDI }
    public interface IMapService
    {
        string Name { get; }
        MapServiceType Type { get; }
        //IServiceMap Map { get; }
    }

    public class ServiceRequest
    {
        public string Service = "";
        public string Request = "";
        public string Response = "";
        public string OnlineResource = "";
        public IIdentity Identity = null;

        public ServiceRequest(string service, string request)
        {
            Service = service;
            Request = request;
        }
    }

    public class InterpreterCapabilities
    {
        public InterpreterCapabilities(Capability[] capabilites)
        {
            Capabilities = capabilites;
        }

        public Capability[] Capabilities
        {
            get;
            private set;
        }

        #region Classes

        public enum Method { Get = 0, Post = 1 };

        public class Capability
        {
            public Capability(string name)
                : this(name, Method.Get, "1.0")
            {
            }
            public Capability(string name, Method method, string version)
            {
                Name = name;
                Method = method;
                Version = version;
            }

            public string Name { get; private set; }
            public Method Method { get; private set; }
            public string Version { get; private set; }
        }

        public class SimpleCapability : Capability
        {
            public SimpleCapability(string name, string link, string version)
                : this(name, Method.Get, link, version)
            {
            }
            public SimpleCapability(string name, Method method, string requestText, string version)
                : base(name, method, version)
            {
                RequestText = requestText;
            }

            public string RequestText { get; private set; }
        }

        public class LinkCapability : Capability
        {
            public LinkCapability(string name, string requestLink, string version)
                : this(name, Method.Get, requestLink, version)
            {
            }
            public LinkCapability(string name, Method method, string requestLink, string version)
                : base(name, method, version)
            {
                RequestLink = requestLink;
            }

            public string RequestLink { get; private set; }
        }

        #endregion
    }

    public interface IServiceRequestInterpreter
    {
        void OnCreate(IMapServer mapServer);
        void Request(IServiceRequestContext context);

        string IntentityName { get; }

        InterpreterCapabilities Capabilities { get; }
    }

    public interface IServiceRequestContext : IContext
    {
        IMapServer MapServer { get; }
        IServiceRequestInterpreter ServiceRequestInterpreter { get; }
        ServiceRequest ServiceRequest { get; }
        IServiceMap ServiceMap { get; }
    }

    public class ServiceRequestContext : IServiceRequestContext
    {
        private IMapServer _mapServer = null;
        private IServiceRequestInterpreter _interpreter = null;
        private ServiceRequest _request = null;

        public ServiceRequestContext(IMapServer mapServer, IServiceRequestInterpreter interpreter, ServiceRequest request)
        {
            _mapServer = mapServer;
            _interpreter = interpreter;
            _request = request;
        }

        #region IServiceRequestContext Member

        public IMapServer MapServer
        {
            get { return _mapServer; }
        }

        public IServiceRequestInterpreter ServiceRequestInterpreter
        {
            get { return _interpreter; }
        }

        public ServiceRequest ServiceRequest
        {
            get { return _request; }
        }
        public IServiceMap ServiceMap
        {
            get { return (_mapServer != null) ? _mapServer[this] : null; }
        }
        #endregion
    }

    /*
    public interface IServiceRequestInterpreterMetadata
    {
        //string MetadataNodeName { get; }
        void ReadMetadta(IServiceMap map, IPersistStream stream);
        void WriteMetadata(IServiceMap map, IPersistStream stream);
    }
     * */
}
